using System;
using CommonObjectPool;
using NS;

namespace GameAbilitySystem.Logic
{
    public class ProviderSetUp
    {
        public AbilityAssetProvider AbilityAssetProvider;
    }
    
    public class GameAbilitySystem:NodeSystem
    {
        //Mgr
        internal ObjectPoolMgr ObjectPoolMgr;
        internal AttributeInstanceMgr AttributeInstanceMgr;
        internal AbilityInstanceMgr AbilityInstanceMgr;

        //Provider
        internal AbilityAssetProvider AbilityAssetProvider;
        
        public override void InitSystem()
        {
            base.InitSystem();
            ObjectPoolMgr = new ObjectPoolMgr();
            AttributeInstanceMgr = new AttributeInstanceMgr(this);
            AbilityInstanceMgr = new AbilityInstanceMgr(this);
        }

        public virtual void SetUpProviders(ProviderSetUp setUp)
        {
            AbilityAssetProvider = setUp.AbilityAssetProvider;
        }
    }
}