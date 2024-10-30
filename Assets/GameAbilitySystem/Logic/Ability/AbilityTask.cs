using System;
using NS;

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