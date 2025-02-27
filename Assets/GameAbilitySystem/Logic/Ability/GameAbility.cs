using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
using GAS.Logic.Value;
using MissQ;
using NS;

namespace GAS.Logic
{
    public struct AbilityCreateParam
    {
        public uint Id;
        public AbilityAsset Asset;
        public uint Lv;
    }

    public enum ECheckAbilityResult
    {
        Success,
        NotAvailable,
        InCooling,
        CostFailed,
    }
    
    public enum EAbilityState
    {
        Initialized,
        Available,
        MarkDestroy,
        UnInitialized,
    }
    
    public class GameAbility :IPoolObject, ITickable
    {
        internal AbilityAsset Asset;
        internal readonly GameAbilityGraphController GraphController = new();
        internal uint Lv;
        public GameUnit Owner { get; private set; }
        internal uint ID { get; private set; }
        internal EAbilityState State { get; private set; }

        internal Action OnAdd;
        internal Action OnRemove;
        
        //Cooldown
        internal bool IsInCooldown;
        internal FP CooldownDuration;
        internal FP CooldownCounter;
        
        private bool IsAvailable => State == EAbilityState.Available;
        private readonly List<NodeGraphRunner> _activateAbilityRunners = new();
        
        //在Job完成或Ability取消时需要移除。取消时，Job被标记为已取消，但还在AbilityActivationReqSubsystem的Queue里，只有当轮到它执行时，才真正移除
        private readonly List<AbilityActivationReqJob> _activationReqJobs = new();
            
        internal GameAbilitySystem System { get; private set; }
        internal bool IsAbilityInActivating => _activateAbilityRunners.Count > 0;
        
        public string AbilityName => Asset?.abilityName ?? string.Empty;
        
        internal void Init(GameAbilitySystem sys, ref AbilityCreateParam param)
        {
            ID = param.Id;
            Asset = param.Asset;
            GraphController.Init(sys, Asset, this);
            Lv = param.Lv;
            
            State = EAbilityState.Initialized;
            System = sys;
        }

        private void UnInit()
        {
            CancelAllActivationReqJobs();
            _activateAbilityRunners.Clear();
            ResetCooldown();
            GraphController.UnInit();
            State = EAbilityState.UnInitialized;
        }
        
        #region Object Pool

        public void OnCreateFromPool(ObjectPool pool)
        {
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            UnInit();
        }

        public void OnDestroy()
        {
        }

        #endregion

        #region Tick

        public void TickCooldown(float deltaTime)
        {
            if (!IsInCooldown) 
                return;
            CooldownCounter += deltaTime;
            if (CooldownCounter >= CooldownDuration)
            {
                ResetCooldown();
            }
        }
        
        public void OnTick(float deltaTime)
        {
            
        }
        
        #endregion
        
        //变更等级
        internal void ChangeAbilityLevel(uint newLv)
        {
            if(!IsAvailable)
                return;
            Lv = newLv;
        }
        
        //获得和移除Ability
        internal void OnAddAbility(GameUnit owner)
        {
            Owner = owner;
            GameLogger.Log($"On add ability: {AbilityName} of {Owner.UnitName}");
            State = EAbilityState.Available;
            OnAdd?.Invoke();
            
            GraphController.RunGraph(typeof(OnAddAbilityPortalNode));
            //todo: Graph register to game event
        }

        internal void OnRemoveAbility()
        {
            GameLogger.Log($"On remove ability: {AbilityName} of {Owner.UnitName}");
            //todo: Graph unregister to game event
            
            GraphController.RunGraph(typeof(OnRemoveAbilityPortalNode));
            OnRemove?.Invoke();
            Owner = null;
            State = EAbilityState.MarkDestroy;
        }
        
        //检测技能执行条件是否满足
        private ECheckAbilityResult CheckAbility()
        {
            //Available
            if(!IsAvailable)
                return ECheckAbilityResult.NotAvailable;
            
            //Cooldown
            if (IsInCooldown)
                return ECheckAbilityResult.InCooling;

            //Cost 
            foreach (var costElement in Asset.costs)
            {
                var costNums = ValuePickerUtility.GetValue(costElement.costVal, Owner, Lv);
                if (Owner.GetSimpleAttributeVal(costElement.attributeType) < costNums)
                    return ECheckAbilityResult.CostFailed;
            }

            return ECheckAbilityResult.Success;
        }

        //提交执行技能的消耗，并开始冷却计时
        private void CommitAbility()
        {
            StartCooldown();
            
            foreach (var costElement in Asset.costs)
            {
                var costNums = ValuePickerUtility.GetValue(costElement.costVal, Owner, Lv);
                var attribute = Owner.GetSimpleAttribute(costElement.attributeType);
                var newVal = attribute.Val - costNums;
                Owner.Sys.GetSubsystem<AttributeInstanceSubsystem>().SetAttributeVal(Owner, attribute, newVal);
            }
        }
        
        #region Cooldown

        private void ResetCooldown()
        {
            CooldownCounter = 0;
            IsInCooldown = false;
        }

        private void StartCooldown()
        {
            CooldownDuration = ValuePickerUtility.GetValue(Asset.cooldown, Owner, Lv);
            if(CooldownDuration > 0)
                IsInCooldown = true;
        }

        #endregion

        #region Activate / Cancel Ability
        
        internal void ActivateAbilityWithGameEventParam(GameEventArg param)
        {
            var checkResult = CheckAbility();
            if (checkResult != ECheckAbilityResult.Success)
            {
                GameLogger.Log($"Activate ability failed, check result : {checkResult}. {AbilityName} of {Owner.UnitName}");
                return;
            }
            
            CommitAbility();
            GameLogger.Log($"Activate ability succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnActivateAbilityPortalNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }

        internal void CancelAbility()
        {
            if (_activateAbilityRunners.Count == 0)
            {
                GameLogger.Log($"Cancel ability failed, not activated. {AbilityName} of {Owner.UnitName}");
                return;
            }

            foreach (var runner in _activateAbilityRunners)
            {
                GameLogger.Log($"Cancel ability runner, portal name:{runner.PortalName}. {AbilityName} of {Owner.UnitName}");
                runner.CancelRunner();
            }

            CancelAllActivationReqJobs();
        }
        
        private void OnActivateAbilityRunnerEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _activateAbilityRunners.Remove(runner);
        }

        internal void ActivateOnStartPreCast(GameEventArg param)
        {
            if(!GraphController.HasPortalNode(typeof(OnStartPreCastAbilityPortalNode)))
                return;
            
            GameLogger.Log($"Activate OnStartPreCast succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnStartPreCastAbilityPortalNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnStartCast(GameEventArg param)
        {
            if(!GraphController.HasPortalNode(typeof(OnStartCastAbilityPortalNode)))
                return;
            
            GameLogger.Log($"Activate OnStartCast succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnStartCastAbilityPortalNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnStartPostCast(GameEventArg param)
        {
            if(!GraphController.HasPortalNode(typeof(OnStartPostCastAbilityPortalNode)))
                return;
            
            GameLogger.Log($"Activate OnStartPostCast succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnStartPostCastAbilityPortalNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnEndPostCast(GameEventArg param)
        {
            if(!GraphController.HasPortalNode(typeof(OnEndPostCastAbilityPortalNode)))
                return;
            
            GameLogger.Log($"Activate OnEndPostCast succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnEndPostCastAbilityPortalNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        #endregion

        #region Activation Req Job

        internal void AddActivationReqJob(AbilityActivationReqJob job)
        {
            if (Owner == null)
            {
                GameLogger.Log($"Add activation req job failed, owner is null. {AbilityName}");
                return;
            }
            _activationReqJobs.Add(job);
            System.GetSubsystem<AbilityActivationReqSubsystem>().EnqueueJob(job);
        }
        
        private void CancelAllActivationReqJobs()
        {
            foreach (var job in _activationReqJobs)
            {
                job.CancelJob();
            }
            _activationReqJobs.Clear();
        }

        #endregion
     
    }
}