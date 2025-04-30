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
        public uint Lv;
        public FP SignalVal1;
        public FP SignalVal2;
        public FP SignalVal3;
        public GameUnit Instigator;
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
    
    public class GameAbility :IPoolClass, IRefCountDisposableObj
    {
        internal AbilityAsset Asset { get; private set; }
        internal readonly GameAbilityGraphController GraphController = new();
        internal uint Lv { get; private set; }
        public GameUnit Owner { get; private set; }
        
        private GameUnit _instigator;
        public GameUnit Instigator => _instigator ?? Owner;
        internal uint ID { get; private set; }
        internal EAbilityState State { get; private set; }
        
        //Cooldown
        internal bool IsInCooldown;
        internal FP CooldownDuration;
        internal FP CooldownCounter;
        
        //SignalVal
        internal FP SignalVal1;
        internal FP SignalVal2;
        internal FP SignalVal3;
        
        private bool IsAvailable => State == EAbilityState.Available;
        private readonly List<NodeGraphRunner> _activateAbilityRunners = new();
        
        //在Job完成或Ability取消时需要移除。取消时，Job被标记为已取消，但还在AbilityActivationReqSubsystem的Queue里，只有当轮到它执行时，才真正移除
        private readonly List<AbilityActivationReqJob> _activationReqJobs = new();
            
        internal GameAbilitySystem System { get; private set; }
        private bool _isActive;
        private RefCountDisposableComponent _refCountDisposableComponent;
        private ClassObjectPool _pool;
        private bool _hasOnTickEntry;
        
        /// <summary>
        /// 技能生效次数
        /// </summary>
        internal int ActivatedCount { get; private set; }
        
        internal bool IsAbilityInActivating => _activateAbilityRunners.Count > 0;
        
        public string AbilityName => Asset?.abilityName ?? string.Empty;
        
        internal void Init(GameAbilitySystem sys,  AbilityAsset asset, ref AbilityCreateParam param)
        {
            ID = param.Id;
            Asset = asset;
            _instigator = param.Instigator;
            Lv = param.Lv;
            
            GraphController.Init(sys, Asset, this);
            
            State = EAbilityState.Initialized;
            System = sys;
            
            SignalVal1 = param.SignalVal1;
            SignalVal2 = param.SignalVal2;
            SignalVal3 = param.SignalVal3;
            
            _instigator?.OnUnitDestroyed.RegisterObserver(this, OnInstigatorDestroy);
        }

        private void UnInit()
        {
            ActivatedCount = 0;
            CancelAllActivationReqJobs();
            _activateAbilityRunners.Clear();
            ResetCooldown();
            GraphController.UnInit();
            State = EAbilityState.UnInitialized;
            Owner = null;
            _hasOnTickEntry = false;
            _instigator?.OnUnitDestroyed.UnRegisterObserver(this);
            _instigator = null;
            Asset = null;
        }
        
        #region Object Pool

        public void OnCreateFromPool(ClassObjectPool pool)
        {
            _pool = pool;
        }

        public void OnTakeFromPool()
        {
            _isActive = true;
        }

        public void OnReturnToPool()
        {
            _isActive = false;
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
                CooldownCounter += System.DeltaTime;
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
                System.GetSubsystem<AbilityInstanceSubsystem>().RemoveFromTickList(this);
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
            Owner = owner;
            GameLogger.Log($"On add ability: {AbilityName} of {Owner.UnitName}");
            State = EAbilityState.Available;
            
            if(GraphController.HasEntryNode(typeof(OnAddAbilityEntryNode)))
                GraphController.RunGraph(typeof(OnAddAbilityEntryNode));

            var gameEventNodeList = GraphController.GetRegisteredGameEventNodePairs();
            if (gameEventNodeList != null)
            {
                foreach (var pair in gameEventNodeList)
                {
                    System.GetSubsystem<GameEventSubsystem>().RegisterGameEvent((EGameEventType)pair.Item1, OnGameEventInvoked);
                }
            }

            _hasOnTickEntry = GraphController.HasEntryNode(typeof(OnTickAbilityEntryNode));
            if (_hasOnTickEntry)
            {
                System.GetSubsystem<AbilityInstanceSubsystem>().AddToTickList(this);
            }
        }

        internal void OnRemoveAbility()
        {
            GameLogger.Log($"On remove ability: {AbilityName} of {Owner.UnitName}");
            
            var gameEventNodeList = GraphController.GetRegisteredGameEventNodePairs();
            if (gameEventNodeList != null)
            {
                foreach (var pair in gameEventNodeList)
                {
                    System.GetSubsystem<GameEventSubsystem>().UnregisterGameEvent((EGameEventType)pair.Item1, OnGameEventInvoked);
                }
            }
            
            if(GraphController.HasEntryNode(typeof(OnRemoveAbilityEntryNode)))
                GraphController.RunGraph(typeof(OnRemoveAbilityEntryNode));
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
                Owner.Sys.GetSubsystem<AttributeInstanceSubsystem>().SetAttributeVal(Owner, costElement.attributeType, newVal);
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
            if (CooldownDuration > 0)
            {
                System.GetSubsystem<AbilityInstanceSubsystem>().AddToTickList(this);
                IsInCooldown = true;
            }
        }

        #endregion

        #region Activate / Cancel / End Ability
        internal void CancelAbility()
        {
            if (_activateAbilityRunners.Count == 0)
            {
                GameLogger.Log($"Cancel ability failed, not activated. {AbilityName} of {Owner.UnitName}");
                return;
            }

            foreach (var runner in _activateAbilityRunners)
            {
                GameLogger.Log($"Cancel ability runner, portal name:{runner.EntryName}. {AbilityName} of {Owner.UnitName}");
                runner.CancelRunner();
            }

            CancelAllActivationReqJobs();
        }

        //结束Ability
        internal void EndAbility()
        {
            Owner.RemoveAbility(this);
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
            
            GameLogger.Log($"Activate OnStartPreCast succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnStartPreCastAbilityEntryNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnStartCast(GameEventArg param)
        {
            if(!GraphController.HasEntryNode(typeof(OnStartCastAbilityEntryNode)))
                return;
            
            GameLogger.Log($"Activate OnStartCast succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnStartCastAbilityEntryNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnStartPostCast(GameEventArg param)
        {
            if(!GraphController.HasEntryNode(typeof(OnStartPostCastAbilityEntryNode)))
                return;
            
            GameLogger.Log($"Activate OnStartPostCast succeeded. {AbilityName} of {Owner.UnitName}");
            var runner = GraphController.RunGraph(typeof(OnStartPostCastAbilityEntryNode), param, OnActivateAbilityRunnerEnd);
            _activateAbilityRunners.Add(runner);
        }
        
        internal void ActivateOnEndPostCast(GameEventArg param)
        {
            if(!GraphController.HasEntryNode(typeof(OnEndPostCastAbilityEntryNode)))
                return;
            
            GameLogger.Log($"Activate OnEndPostCast succeeded. {AbilityName} of {Owner.UnitName}");
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

        #region IRefCountDisposable

        public RefCountDisposableComponent GetRefCountDisposableComponent()
        {
            return _refCountDisposableComponent ?? new RefCountDisposableComponent(this);
        }

        public bool IsDisposed()
        {
            return !_isActive;
        }

        public void ForceDisposeObj()
        {
            GetRefCountDisposableComponent().DisposeOwner();
        }

        public void OnObjDispose()
        {
            GameLogger.Log($"Release Ability: {AbilityName} of {Owner.UnitName}");
            Owner.GameAbilities.Remove(this);
            _pool.Release(this);
        }

        #endregion
        
        
        private void OnInstigatorDestroy(EDestroyUnitReason reason)
        {
            if(!GraphController.HasEntryNode(typeof(OnInstigatorDestroyNode)))
                return;
            
            GameLogger.Log($"On instigator destroy. {AbilityName} of {Owner.UnitName}");
            GraphController.RunGraph(typeof(OnInstigatorDestroyNode));
        }
    }
}