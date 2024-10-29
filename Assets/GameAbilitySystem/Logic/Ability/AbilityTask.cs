using System;
using NodeSystem.ObjectPool;

namespace GameAbilitySystem.Logic.Ability
{
    public class AbilityTask<T> :IPoolObject where T:IEquatable<T>, IComparable<T>
    {
        
        public void Init(AbilityAsset asset)
        {
            
        }
        
        private void UnInit()
        {
            
        }

        internal void TickTask(float dt)
        {
            
        }
        
        #region Pool

        public ObjectPoolParam GetPoolParam()
        {
            return new ObjectPoolParam()
            {
                Capacity = GameAbilitySystemCfg.PoolSizeDefine.DefaultCapacity,
                MaxSize = GameAbilitySystemCfg.PoolSizeDefine.DefaultMaxSize
            };
        }

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