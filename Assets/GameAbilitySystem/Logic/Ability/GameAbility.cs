using System;
using CommonObjectPool;
using GAS.Logic.Value;
using MissQ;

namespace GAS.Logic
{
    public struct AbilityCreateParam
    {
        public uint Id;
        public AbilityAsset Asset;
        public uint Lv;
    }

    public enum EAbilityState
    {
        Initialized,
        Idle,
        Casting,
        MarkDestroy,
        UnInitialized,
    }

    public enum EAbilityCastingState
    {
        None,
        PreCasting,
        Casting,
        PostCasting,
    }
    
    public class GameAbility :IPoolObject, ITickable, IOwnedByGameUnit
    {
        internal AbilityAsset Asset;
        internal readonly GameAbilityGraphController GraphController = new();
        internal uint Lv;
        private GameUnit _owner;
        internal uint ID { get; private set; }
        internal EAbilityState State { get; private set; }
        
        //Cooldown
        internal bool IsInCooldown;
        internal FP CooldownDuration;
        internal FP CooldownCounter;
        
        //Casting
        internal EAbilityCastingState CastingState;
        internal Action OnStartPreCast;
        internal Action OnStartCast;
        internal Action OnStartPostCast;
        internal Action OnEndPostCast;
        
        
        private bool IsAvailable => State == EAbilityState.Idle;
        
        internal void Init(GameAbilitySystem sys, ref AbilityCreateParam param)
        {
            ID = param.Id;
            Asset = param.Asset;
            GraphController.Init(sys, Asset, this);
            Lv = param.Lv;
            
            State = EAbilityState.Initialized;
        }

        private void UnInit()
        {
            ResetCooldown();
            GraphController.UnInit();
            CastingState = EAbilityCastingState.None;
            State = EAbilityState.UnInitialized;
        }

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
            _owner = owner;
            State = EAbilityState.Idle;
            
            GraphController.RunGraph(typeof(OnAddAbilityPortalNode));
            //todo: Graph register to game event
        }

        internal void OnRemoveAbility()
        {
            //todo: Graph unregister to game event
            
            GraphController.RunGraph(typeof(OnRemoveAbilityPortalNode));
            _owner = null;
            State = EAbilityState.MarkDestroy;
        }
        
        //检测技能执行条件是否满足
        internal bool CheckAbility()
        {
            if(!IsAvailable)
                return false;
            
            //Cooldown
            if (IsInCooldown)
                return false;

            //Cost 
            foreach (var costElement in Asset.costs)
            {
                var costNums = ValuePickerUtility.GetValue(costElement.costVal, _owner, Lv);
                if (_owner.GetSimpleAttributeVal(costElement.attributeType) < costNums)
                    return false;
            }

            return true;
        }

        //提交执行技能的消耗，并开始冷却计时
        internal bool CommitAbility()
        {
            if (!CheckAbility())
                return false;
            
            StartCooldown();
            
            foreach (var costElement in Asset.costs)
            {
                var costNums = ValuePickerUtility.GetValue(costElement.costVal, _owner, Lv);
                var attribute = _owner.GetSimpleAttribute(costElement.attributeType);
                var newVal = attribute.Val - costNums;
                _owner.Sys.AttributeInstanceMgr.SetAttributeVal(_owner, attribute, newVal);
            }

            return true;
        }
        
        internal void ActivateAbility()
        {
            if(!IsAvailable)
                return;
            
            GraphController.RunGraph(typeof(OnActivateAbilityPortalNode));
        }

        #region Cooldown

        private void ResetCooldown()
        {
            CooldownCounter = 0;
            IsInCooldown = false;
        }

        private void StartCooldown()
        {
            CooldownDuration = ValuePickerUtility.GetValue(Asset.cooldown, _owner, Lv);
            if(CooldownDuration > 0)
                IsInCooldown = true;
        }

        #endregion

        #region Casting

        private void StartPreCast()
        {
            CastingState = EAbilityCastingState.PreCasting;
            OnStartPreCast?.Invoke();
        }

        private void StartCast()
        {
            CastingState = EAbilityCastingState.Casting;
            OnStartCast?.Invoke();
        }

        private void StartPostCast()
        {
            CastingState = EAbilityCastingState.PostCasting;
            OnStartPostCast?.Invoke();
        }

        private void EndPostCast()
        {
            CastingState = EAbilityCastingState.None;
            OnEndPostCast?.Invoke();
        }

        #endregion
       
        
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

        public GameUnit GetOwner()
        {
            return _owner;
        }
    }
}