using CommonObjectPool;
using MissQ;

namespace GameAbilitySystem.Logic
{
    public struct AbilityCreateParam
    {
        public uint Id;
        public AbilityAsset Asset;
    }
    
    public class GameAbility :IPoolObject
    {
        internal AbilityAsset Asset;
        internal readonly GameAbilityGraphController GraphController = new();
        internal GameUnit Owner;
        
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
        }

        private void UnInit()
        {
            GraphController.UnInit();
        }

        internal void UpdateAbility(FP dt)
        {
            if (IsInCooldown)
            {
                CooldownCounter += dt;
                if (CooldownCounter >= Cooldown)
                {
                    CooldownCounter = 0;
                    IsInCooldown = false;
                }
            }
            
            GraphController.UpdateGraphs((float)dt);
        }
        
        //获得和移除Ability
        internal void OnAddAbility(GameUnit owner)
        {
            Owner = owner;
            GraphController.RunGraph(EDefaultEvent.OnAddAbility);
            
            //todo: Register to game event
        }

        internal void OnRemoveAbility()
        {
            //todo: Unregister to game event
            
            GraphController.RunGraph(EDefaultEvent.OnRemoveAbility);
            Owner = null;
        }
        
        //检测技能执行条件是否满足
        internal virtual bool CheckAbility()
        {
            return true;
        }

        //提交执行技能的消耗，并开始冷却计时
        internal virtual void CommitAbility()
        {
            CooldownCounter = 0;
            IsInCooldown = true;
            
        }

        //非事件触发
        // 1. 玩家主动触发
        // 2. 时间触发
        internal void ActivateAbility()
        {
            if (!CheckAbility())
                return;
            
            
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