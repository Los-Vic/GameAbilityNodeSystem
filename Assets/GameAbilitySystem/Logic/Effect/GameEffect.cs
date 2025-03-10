using GameplayCommonLibrary;

namespace GAS.Logic
{
    
    public struct EffectCreateParam
    {
        public uint Id;
        public EffectAsset Asset;
        public uint Lv;
    }
    
    public class GameEffect:IPoolClass
    {
        internal uint Lv { get; private set; }
        internal EffectAsset Asset { get; private set; }
        internal uint ID { get; private set; }
        internal GameAbilitySystem System { get; private set; }
        public GameUnit Owner { get; private set; }
        
        internal void Init(GameAbilitySystem sys, ref EffectCreateParam param)
        {
            ID = param.Id;
            Asset = param.Asset;
            //GraphController.Init(sys, Asset, this);
            Lv = param.Lv;
            System = sys;
        }
        
        private void UnInit()
        {
            Owner = null;
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