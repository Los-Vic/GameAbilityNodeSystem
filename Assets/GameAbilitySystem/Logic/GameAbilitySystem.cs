using CommonObjectPool;
using NS;

namespace GAS.Logic
{
    public class GameAbilitySystem:NodeSystem
    {
        //Mgr
        internal ObjectPoolMgr ObjectPoolMgr { get; private set; }
        internal AttributeInstanceMgr AttributeInstanceMgr { get; private set; }
        internal AbilityInstanceMgr AbilityInstanceMgr { get; private set; }
        //Provider
        internal IAssetConfigProvider AssetConfigProvider { get; private set; }
        
        public GameAbilitySystem(IAssetConfigProvider provider)
        {
            AssetConfigProvider = provider;
        }
        
        public override void InitSystem()
        {
            base.InitSystem();
            ObjectPoolMgr = new ObjectPoolMgr();
            AttributeInstanceMgr = new AttributeInstanceMgr(this);
            AbilityInstanceMgr = new AbilityInstanceMgr(this);
        }

        public override void UnInitSystem()
        {
            base.UnInitSystem();
        }

        public void Update(float deltaTime)
        {
            
        }
    }
}