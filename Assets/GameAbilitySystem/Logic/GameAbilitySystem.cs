using System;
using CommonObjectPool;
using GameAbilitySystem.Logic.Ability;
using GameAbilitySystem.Logic.Attribute;
using NS;

namespace GameAbilitySystem.Logic
{
    public class ProviderSetUp
    {
        public AbilityAssetProvider AbilityAssetProvider;
    }
    
    public class GameAbilitySystem<T> where T:IEquatable<T>, IComparable<T>
    {
        //Mgr
        internal readonly ObjectPoolMgr ObjectPoolMgr;
        internal readonly AttributeInstanceMgr<T> AttributeInstanceMgr;
        internal readonly AbilityInstanceMgr<T> AbilityInstanceMgr;

        //Provider
        internal AbilityAssetProvider AbilityAssetProvider;
        
        protected GameAbilitySystem()
        {
            ObjectPoolMgr = new ObjectPoolMgr();
            AttributeInstanceMgr = new AttributeInstanceMgr<T>(this);
            AbilityInstanceMgr = new AbilityInstanceMgr<T>(this);
        }

        public virtual void SetUpProviders(ProviderSetUp setUp)
        {
            AbilityAssetProvider = setUp.AbilityAssetProvider;
        }
    }
}