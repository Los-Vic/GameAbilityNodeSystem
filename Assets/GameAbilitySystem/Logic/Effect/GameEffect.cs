using System;
using CommonObjectPool;
using NS;

namespace GameAbilitySystem.Logic
{
    public class GameEffect:IPoolObject
    {
        private void UnInit()
        {
            
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