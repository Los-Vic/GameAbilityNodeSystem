using System;
using CommonObjectPool;
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
    
    public class GameAbility :IPoolObject, ITickable
    {
        internal AbilityAsset Asset;
        internal readonly GameAbilityGraphController GraphController = new();
        internal uint Lv;
        public GameUnit Owner { get; private set; }
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
            NodeSystemLogger.Log($"On add ability: {Asset.abilityName}");
            Owner = owner;
            State = EAbilityState.Idle;
            
            GraphController.RunGraph(typeof(OnAddAbilityPortalNode));
            //todo: Graph register to game event
        }

        internal void OnRemoveAbility()
        {
            NodeSystemLogger.Log($"On remove ability: {Asset.abilityName}");
            //todo: Graph unregister to game event
            
            GraphController.RunGraph(typeof(OnRemoveAbilityPortalNode));
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
                Owner.Sys.AttributeInstanceMgr.SetAttributeVal(Owner, attribute, newVal);
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

        #region Graph Funciton

        internal void GF_ActivateAbility()
        {
            var checkResult = CheckAbility();
            if (checkResult != ECheckAbilityResult.Success)
            {
                NodeSystemLogger.Log($"Activate ability failed, check result : {checkResult}. {Asset.abilityName}");
                return;
            }
            
            CommitAbility();
            NodeSystemLogger.Log($"Activate ability succeeded. {Asset.abilityName}");
            GraphController.RunGraph(typeof(OnActivateAbilityPortalNode));
        }

        internal void GF_ActivateAbilityWithGameEventParam(GameEventNodeParam param)
        {
            if (param == null)
            {
                NodeSystemLogger.Log($"Activate ability with param failed, param is null. {Asset.abilityName}");
                return;
            }
            
            var checkResult = CheckAbility();
            if (checkResult != ECheckAbilityResult.Success)
            {
                NodeSystemLogger.Log($"Activate ability with param failed, check result : {checkResult}. {Asset.abilityName}");
                return;
            }
            
            CommitAbility();
            NodeSystemLogger.Log($"Activate ability with param succeeded. {Asset.abilityName}");
            GraphController.RunGraph(typeof(OnActivateAbilityByEventPortalNode), param);
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
    }
}