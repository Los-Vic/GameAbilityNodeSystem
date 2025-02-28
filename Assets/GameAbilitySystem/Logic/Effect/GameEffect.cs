using System;
using GameplayCommonLibrary;
using NS;

namespace GAS.Logic
{
    public class GameEffect:IPoolClass
    {
        private void UnInit()
        {
            
        }
        #region Object Pool

        public void OnCreateFromPool(ClassObjectPool pool)
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