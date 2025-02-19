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
        private GameAbilitySystem _system;
        internal bool IsAbilityInActivating => _activateAbilityRunners.Count > 0;
        
        public string AbilityName => Asset?.abilityName ?? string.Empty;
        
        internal void Init(GameAbilitySystem sys, ref AbilityCreateParam param)
        {
            ID = param.Id;
            Asset = param.Asset;
            GraphController.Init(sys, Asset, this);
            Lv = param.Lv;
            
            State = EAbilityState.Initialized;
            _system = sys;
        }

        private void UnInit()
        {
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
            _system.Logger.Log($"On add ability: {AbilityName} of {Owner.UnitName}");
            State = EAbilityState.Available;
            OnAdd?.Invoke();
            
            GraphController.RunGraph(typeof(OnAddAbilityPortalNode));
            //todo: Graph register to game event
        }

        internal void OnRemoveAbility()
        {
            _system.Logger.Log($"On remove ability: {AbilityName} of {Owner.UnitName}");
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

        #region Graph Funciton

        internal void GF_ActivateAbility()
        {
            var checkResult = CheckAbility();
            if (checkResult != ECheckAbilityResult.Success)
            {
                _system.Logger.Log($"Activate ability failed, check result : {checkResult}. {AbilityName} of {Owner.UnitName}");
                return;
            }
            
            CommitAbility();
            _system.Logger.Log($"Activate ability succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnActivateAbilityPortalNode), null, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }

        internal void GF_ActivateAbilityWithGameEventParam(GameEventArg param)
        {
            if (param == null)
            {
                _system.Logger.Log($"Activate ability with param failed, param is null. {AbilityName} of {Owner.UnitName}");
                return;
            }
            
            var checkResult = CheckAbility();
            if (checkResult != ECheckAbilityResult.Success)
            {
                _system.Logger.Log($"Activate ability with param failed, check result : {checkResult}. {AbilityName} of {Owner.UnitName}");
                return;
            }
            
            CommitAbility();
            _system.Logger.Log($"Activate ability with param succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnActivateAbilityByEventPortalNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }

        internal void GF_CancelAbility()
        {
            if (_activateAbilityRunners.Count == 0)
            {
                _system.Logger.Log($"Cancel ability failed, not activated. {AbilityName} of {Owner.UnitName}");
                return;
            }

            foreach (var runner in _activateAbilityRunners)
            {
                _system.Logger.Log($"Cancel ability runner, portal name:{runner.PortalName}. {AbilityName} of {Owner.UnitName}");
                runner.CancelRunner();
            }
        }
        
        #endregion

        private void OnActivateAbilityRunnerEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _activateAbilityRunners.Remove(runner);
        }
    }
}