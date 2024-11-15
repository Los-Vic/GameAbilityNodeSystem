using System;
using CommonObjectPool;
using NS;

namespace GameAbilitySystem.Logic
{
    public class ProviderSetUp
    {
        public AbilityAssetProvider AbilityAssetProvider;
    }
    
    public class GameAbilitySystem
    {
        //Mgr
        internal readonly ObjectPoolMgr ObjectPoolMgr;
        internal readonly AttributeInstanceMgr AttributeInstanceMgr;
        internal readonly AbilityInstanceMgr AbilityInstanceMgr;
        internal readonly GameAbilityNodeSystem NodeSystem;

        //Provider
        internal AbilityAssetProvider AbilityAssetProvider;
        
        protected GameAbilitySystem()
        {
            ObjectPoolMgr = new ObjectPoolMgr();
            AttributeInstanceMgr = new AttributeInstanceMgr(this);
            AbilityInstanceMgr = new AbilityInstanceMgr(this);
            NodeSystem = new GameAbilityNodeSystem();
        }

        public virtual void SetUpProviders(ProviderSetUp setUp)
        {
            AbilityAssetProvider = setUp.AbilityAssetProvider;
        }
    }
}