using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    
    public struct EffectCreateParam
    {
        public EffectAsset Asset;
        public uint Lv;
        public FP SignalVal1;
        public FP SignalVal2;
        public FP SignalVal3;
    }
    
    public class GameEffect:IPoolClass, IRefCountDisposableObj
    {
        internal uint Lv { get; private set; }
        internal EffectAsset Asset { get; private set; }
        internal GameAbilitySystem System { get; private set; }
        public GameUnit Owner { get; private set; }
        public string EffectName => Asset?.effectName ?? string.Empty;
        
        private RefCountDisposableComponent _refCountDisposableComponent;
        private bool _isActive;
        private ClassObjectPool _pool;
        
        internal void Init(GameAbilitySystem sys, ref EffectCreateParam param)
        {
            Asset = param.Asset;
            //GraphController.Init(sys, Asset, this);
            Lv = param.Lv;
            System = sys;
        }
        
        private void UnInit()
        {
            Owner = null;
        }

        internal void OnAddEffect(GameUnit owner)
        {
            Owner = owner;
            GameLogger.Log($"On add effect: {EffectName} of {Owner.UnitName}");
        }

        internal void OnRemoveEffect()
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
            GameLogger.Log($"Release Effect: {EffectName} of {Owner.UnitName}");
            Owner.GameEffects.Remove(this);
            _pool.Release(this);
        }

        #endregion
      
    }
}