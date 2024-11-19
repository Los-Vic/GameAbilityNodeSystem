using CommonObjectPool;
using MissQ;

namespace GAS.Logic
{
    public struct AbilityCreateParam
    {
        public uint Id;
        public AbilityAsset Asset;
        public uint Lv;
    }
    
    public class GameAbility :IPoolObject, ITickable
    {
        internal AbilityAsset Asset;
        internal readonly GameAbilityGraphController GraphController = new();
        internal GameUnit Owner;
        internal uint Lv;
        
        //Cooldown
        internal bool IsInCooldown;
        internal FP Cooldown;
        internal FP CooldownCounter;
        
        internal uint ID { get; private set; }
        
        internal void Init(GameAbilitySystem sys, ref AbilityCreateParam param)
        {
            ID = param.Id;
            Asset = param.Asset;
            GraphController.Init(sys, Asset);
            Lv = param.Lv;
        }

        private void UnInit()
        {
            GraphController.UnInit();
        }

        public void OnTick(float deltaTime)
        {
            if (IsInCooldown)
            {
                CooldownCounter += deltaTime;
                if (CooldownCounter >= Cooldown)
                {
                    ResetCooldown();
                }
            }
            
            GraphController.UpdateGraphs(deltaTime);
        }
        
        //获得和移除Ability
        internal void OnAddAbility(GameUnit owner)
        {
            Owner = owner;
            GraphController.RunGraph(EDefaultEvent.OnAddAbility);
            
            //todo: Graph register to game event
        }

        internal void OnRemoveAbility()
        {
            //todo: Graph unregister to game event
            
            GraphController.RunGraph(EDefaultEvent.OnRemoveAbility);
            Owner = null;
        }
        
        //检测技能执行条件是否满足
        internal bool CheckAbility()
        {
            //Cooldown
            if (IsInCooldown)
                return false;

            //Cost
            foreach (var costElement in Asset.costs)
            {
                var costNums = Owner.Sys.AssetConfigProvider.GetAbilityEffectParamVal(costElement.costVal, Lv);
                if (Owner.GetSimpleAttributeValue(costElement.attributeType) < costNums)
                    return false;
            }

            return true;
        }

        //提交执行技能的消耗，并开始冷却计时
        internal void CommitAbility()
        {
            ResetCooldown();
            
            foreach (var costElement in Asset.costs)
            {
                var costNums = Owner.Sys.AssetConfigProvider.GetAbilityEffectParamVal(costElement.costVal, Lv);
                var attribute = Owner.GetSimpleAttribute(costElement.attributeType);
                var newVal = attribute.Val - costNums;
                Owner.Sys.AttributeInstanceMgr.SetAttributeVal(Owner, attribute, newVal);
            }
        }

        //非事件触发
        // 1. 玩家主动触发
        // 2. 时间触发
        internal void ActivateAbility()
        {
            if (!CheckAbility())
                return;

            CommitAbility();
            GraphController.RunGraph(EDefaultEvent.OnActivateAbility);
        }

        internal void ResetCooldown()
        {
            CooldownCounter = 0;
            IsInCooldown = false;
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
    }
}