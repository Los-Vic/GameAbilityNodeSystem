using GCL;

namespace GAS.Logic
{
    public class GameAbilitySystemObject:IPoolObject
    {
        public GameAbilitySystem System { get; internal set; }
        
        #region IPoolClass
        public virtual void OnCreateFromPool()
        {
        }

        public virtual void OnTakeFromPool()
        {
        }

        public virtual void OnReturnToPool()
        {
        }

        public virtual void OnDestroy()
        {
        }

        #endregion
    }
}