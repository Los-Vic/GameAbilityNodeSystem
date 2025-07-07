using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;
using GAS.Logic.Value;
using MissQ;
using NS;

namespace GAS.Logic
{
    public struct AbilityCreateParam
    {
        public uint Id;
        public uint Lv;
        public FP SignalVal1;
        public FP SignalVal2;
        public FP SignalVal3;
        public Handler<GameUnit> Instigator;
    }

    public struct AbilityInitParam
    {
        public AbilityCreateParam CreateParam;
        public Handler<GameAbility> Handler;
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
    
    public class GameAbility :IPoolClass
    {
        public Handler<GameAbility> Handler { get; private set; }
        internal AbilityAsset Asset { get; private set; }
        internal readonly GameAbilityGraphController GraphController = new();
        public uint Lv { get; private set; }
        public Handler<GameUnit> Owner { get; private set; }
        public Handler<GameUnit> Instigator { get; private set; }
        internal uint ID { get; private set; }
        internal EAbilityState State { get; private set; }
        
        //Cooldown
        internal bool IsInCooldown;
        internal FP CooldownDuration;
        internal FP CooldownCounter;
        
        //SignalVal, used for passing value between abilities
        internal FP SignalVal1;
        internal FP SignalVal2;
        internal FP SignalVal3;
        
        //CustomVal
        internal FP CustomVal;
        
        private bool IsAvailable => State == EAbilityState.Available;
        private readonly List<NodeGraphRunner> _activateAbilityRunners = new();
        
        //在Job完成或Ability取消时需要移除。取消时，Job被标记为已取消，但还在AbilityActivationReqSubsystem的Queue里，只有当轮到它执行时，才真正移除
        private readonly List<AbilityActivationReqJob> _activationReqJobs = new();
            
        public GameAbilitySystem Sys { get; private set; }
        private bool _hasOnTickEntry;
        
        /// <summary>
        /// 技能生效次数
        /// </summary>
        internal int ActivatedCount { get; private set; }
        
        internal bool IsAbilityInActivating => _activateAbilityRunners.Count > 0;

        public string AbilityName { get; private set; }


        internal void Init(GameAbilitySystem sys,  AbilityAsset asset, ref AbilityInitParam param)
        {
            ID = param.CreateParam.Id;
            Asset = asset;
            Instigator = param.CreateParam.Instigator;
            Lv = param.CreateParam.Lv;
            Handler = param.Handler;
            
            GraphController.Init(sys, Asset, this);
            
            State = EAbilityState.Initialized;
            Sys = sys;
            
            SignalVal1 = param.CreateParam.SignalVal1;
            SignalVal2 = param.CreateParam.SignalVal2;
            SignalVal3 = param.CreateParam.SignalVal3;

            if (sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(param.CreateParam.Instigator, out var instigator))
            {
                instigator.OnUnitDestroyed.RegisterObserver(this, OnInstigatorDestroy);
            }
        }

        private void UnInit()
        {
            ActivatedCount = 0;
            CancelAllActivationReqJobs();
            _activateAbilityRunners.Clear();
            ResetCooldown();
            GraphController.UnInit();
            State = EAbilityState.UnInitialized;
            Owner = 0;
            _hasOnTickEntry = false;
            
            if (Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Instigator, out var instigator))
            {
                instigator.OnUnitDestroyed.UnRegisterObserver(this);
            }
            Instigator = 0;
            Asset = null;
            AbilityName = string.Empty;
        }

        public override string ToString()
        {
            return AbilityName;
        }

        #region Object Pool

        public void OnCreateFromPool()
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
        
        public void OnTick()
        {
            if (IsInCooldown)
            {
                CooldownCounter += Sys.DeltaTime;
                if (CooldownCounter >= CooldownDuration)
                {
                    ResetCooldown();
                }
            }
            
            if (_hasOnTickEntry)
            {
                GraphController.RunGraph(typeof(OnTickAbilityEntryNode));
            }
            else if (!IsInCooldown)
            {
                Sys.AbilityInstanceSubsystem.RemoveFromTickList(this);
            }
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
            Owner = owner.Handler;
            AbilityName = $"{Asset.abilityName} of {owner}";
            GameLogger.Log($"On add ability: {AbilityName}");
            State = EAbilityState.Available;
            
            if(GraphController.HasEntryNode(typeof(OnAddAbilityEntryNode)))
                GraphController.RunGraph(typeof(OnAddAbilityEntryNode));

            var gameEventNodeList = GraphController.GetRegisteredGameEventNodePairs();
            if (gameEventNodeList != null)
            {
                foreach (var pair in gameEventNodeList)
                {
                    Sys.GameEventSubsystem.RegisterGameEvent((EGameEventType)pair.Item1, OnGameEventInvoked);
                }
            }

            _hasOnTickEntry = GraphController.HasEntryNode(typeof(OnTickAbilityEntryNode));
            if (_hasOnTickEntry)
            {
                Sys.AbilityInstanceSubsystem.AddToTickList(this);
            }
        }

        internal void OnRemoveAbility()
        {
            GameLogger.Log($"On remove ability: {AbilityName}");
            
            var gameEventNodeList = GraphController.GetRegisteredGameEventNodePairs();
            if (gameEventNodeList != null)
            {
                foreach (var pair in gameEventNodeList)
                {
                    Sys.GameEventSubsystem.UnregisterGameEvent((EGameEventType)pair.Item1, OnGameEventInvoked);
                }
            }
            
            if(GraphController.HasEntryNode(typeof(OnRemoveAbilityEntryNode)))
                GraphController.RunGraph(typeof(OnRemoveAbilityEntryNode));
        }

        internal void MarkDestroy()
        {
            State = EAbilityState.MarkDestroy;
        }

        private void OnGameEventInvoked(GameEventArg arg)
        {
           GraphController.RunGraphGameEvent(arg.EventType, arg);
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
                if(!Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Owner, out var owner))
                    continue;
                var costNums = ValuePickerUtility.GetValue(costElement.costVal, owner, Lv);
                if (owner.GetSimpleAttributeVal(costElement.attributeType) < costNums)
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
                if(!Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Owner, out var owner))
                    continue;
                var costNums = ValuePickerUtility.GetValue(costElement.costVal, owner, Lv);
                var attribute = owner.GetSimpleAttribute(costElement.attributeType);
                var newVal = attribute.Val - costNums;
                Sys.AttributeInstanceSubsystem.SetAttributeVal(owner, costElement.attributeType, newVal);
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
            if (!Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Owner, out var owner))
                return;
            
            CooldownDuration = ValuePickerUtility.GetValue(Asset.cooldown, owner, Lv);
            if (CooldownDuration > 0)
            {
                Sys.AbilityInstanceSubsystem.AddToTickList(this);
                IsInCooldown = true;
            }
        }

        #endregion

        #region Activate / Cancel / End Ability
        internal void CancelAbility()
        {
            if (_activateAbilityRunners.Count == 0)
            {
                GameLogger.Log($"Cancel ability failed, not activated. {AbilityName}");
                return;
            }

            foreach (var runner in _activateAbilityRunners)
            {
                GameLogger.Log($"Cancel ability runner, portal name:{runner.EntryName}. {AbilityName}");
                runner.CancelRunner();
            }

            CancelAllActivationReqJobs();
        }

        //结束Ability
        internal void EndAbility()
        {
            if(Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Owner, out var owner))
                owner.RemoveAbility(this);
        }
        
        private void OnActivateAbilityRunnerEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _activateAbilityRunners.Remove(runner);
        }

        internal void ActivateOnStartPreCast(GameEventArg param)
        {
            ActivatedCount++;
            
            if(!GraphController.HasEntryNode(typeof(OnStartPreCastAbilityEntryNode)))
                return;
            
            GameLogger.Log($"Activate OnStartPreCast succeeded. {AbilityName}");
            var runner = GraphController.RunGraph(typeof(OnStartPreCastAbilityEntryNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnStartCast(GameEventArg param)
        {
            if(!GraphController.HasEntryNode(typeof(OnStartCastAbilityEntryNode)))
                return;
            
            GameLogger.Log($"Activate OnStartCast succeeded. {AbilityName}");
            var runner = GraphController.RunGraph(typeof(OnStartCastAbilityEntryNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnStartPostCast(GameEventArg param)
        {
            if(!GraphController.HasEntryNode(typeof(OnStartPostCastAbilityEntryNode)))
                return;
            
            GameLogger.Log($"Activate OnStartPostCast succeeded. {AbilityName}");
            var runner = GraphController.RunGraph(typeof(OnStartPostCastAbilityEntryNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnEndPostCast(GameEventArg param)
        {
            if(!GraphController.HasEntryNode(typeof(OnEndPostCastAbilityEntryNode)))
                return;
            
            GameLogger.Log($"Activate OnEndPostCast succeeded. {AbilityName}");
            var runner = GraphController.RunGraph(typeof(OnEndPostCastAbilityEntryNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        #endregion

        #region Activation Req Job

        internal void AddActivationReqJob(AbilityActivationReqJob job)
        {
            var checkRes = CheckAbility();
            if (checkRes != ECheckAbilityResult.Success)
            {
                GameLogger.Log($"Failed to add activation req job, check ability : {checkRes}");
                return;
            }
            
            CommitAbility();
            
            if (!Sys.GetRscFromHandler(Owner, out var owner))
            {
                GameLogger.Log($"Add activation req job failed, owner is null. {AbilityName}");
                return;
            }
            _activationReqJobs.Add(job);
            Sys.AbilityActivationReqSubsystem.EnqueueJob(job);
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
        
        private void OnInstigatorDestroy(EDestroyUnitReason reason)
        {
            if(!GraphController.HasEntryNode(typeof(OnInstigatorDestroyNode)))
                return;
            
            GameLogger.Log($"On instigator destroy. {AbilityName}");
            GraphController.RunGraph(typeof(OnInstigatorDestroyNode));
        }
    }
}