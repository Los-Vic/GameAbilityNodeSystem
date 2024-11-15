using System;
using CommonObjectPool;

namespace GameAbilitySystem.Logic
{
    public struct AbilityCreateParam
    {
        public AbilityAsset Asset;
    }
    
    public class GameAbility :IPoolObject
    {
        internal AbilityAsset Asset;
        internal readonly GameAbilityGraphController GraphController = new();
        internal GameUnit Owner;
        
        public uint ID => Asset?.abilityID ?? 0;
        
        internal void Init(GameAbilitySystem sys, ref AbilityCreateParam param)
        {
            Asset = param.Asset;
            GraphController.Init(sys, Asset);
        }

        private void UnInit()
        {
            GraphController.UnInit();
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
        
        //执行Ability
        public virtual bool CheckAbility()
        {
            return true;
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