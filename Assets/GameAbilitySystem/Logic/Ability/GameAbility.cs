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
        public uint ID => Asset?.abilityID ?? 0;

        internal void Init(ref AbilityCreateParam param)
        {
            Asset = param.Asset;
        }

        private void UnInit()
        {
            
        }

        //获得和移除Ability
        internal void OnAddAbility()
        {
            
        }

        internal void OnRemoveAbility()
        {
            
        }
        
        //如何触发Ability
        
        
        
        //执行Ability
        public virtual bool CheckAbility()
        {
            return true;
        }

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