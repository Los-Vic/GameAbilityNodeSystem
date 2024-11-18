using System;
using CommonObjectPool;
using NS;

namespace GAS.Logic
{
    public class AbilityTask
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