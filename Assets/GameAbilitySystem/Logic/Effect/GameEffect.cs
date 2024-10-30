using System;
using NS;

namespace GameAbilitySystem.Logic.Effect
{
    public class GameEffect<T>:IPoolObject where T:IEquatable<T>, IComparable<T>
    {
        private void UnInit()
        {
            
        }
        #region Object Pool

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