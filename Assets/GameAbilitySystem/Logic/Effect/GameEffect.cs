using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class GameEffect:IPoolClass, IRefCountDisposableObj
    {
        public GameUnit Owner { get; private set; }
        public string EffectName;
        
        private RefCountDisposableComponent _refCountDisposableComponent;
        private bool _isActive;
        private ClassObjectPool _pool;
        
        internal void Init(string effectName)
        {
            EffectName = effectName;
        }
        
        internal virtual void UnInit()
        {
            Owner = null;
            EffectName = string.Empty;
        }
        
        internal virtual void OnAddEffect(GameUnit owner)
        {
            Owner = owner;
            GameLogger.Log($"On add effect: {EffectName} of {Owner.UnitName}");
        }

        internal virtual void OnRemoveEffect()
        {
            GameLogger.Log($"On remove effect: {EffectName} of {Owner.UnitName}");
        }
        
        #region Object Pool

        public void OnCreateFromPool(ClassObjectPool pool)
        {
            _pool = pool;
        }

        public void OnTakeFromPool()
        {
            _isActive = true;
        }

        public void OnReturnToPool()
        {
            _isActive = false;
            UnInit();
        }

        public void OnDestroy()
        {
        }

        #endregion

        #region IRefCountDisposableObj

        public RefCountDisposableComponent GetRefCountDisposableComponent()
        {
            return _refCountDisposableComponent ?? new RefCountDisposableComponent(this);
        }

        public bool IsDisposed()
        {
            return !_isActive;
        }

        public void ForceDisposeObj()
        {
            GetRefCountDisposableComponent().DisposeOwner();
        }

        public void OnObjDispose()
        {
            GameLogger.Log($"Release Effect: {EffectName} of {Owner?.UnitName}");
            Owner?.GameEffects.Remove(this);
            _pool.Release(this);
        }

        #endregion
      
    }
}