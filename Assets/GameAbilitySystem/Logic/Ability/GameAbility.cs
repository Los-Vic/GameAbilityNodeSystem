using System;
using GameAbilitySystem.Logic.ObjectPool;

namespace GameAbilitySystem.Logic.Ability
{
    public struct AbilityCreateParam
    {
        public AbilityAsset Asset;
    }
    
    public class GameAbility<T> :IPoolObject where T:IEquatable<T>, IComparable<T>
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