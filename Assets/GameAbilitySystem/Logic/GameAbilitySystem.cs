using CommonObjectPool;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    public class GameAbilitySystem:NodeSystem
    {
        //Mgr
        internal ObjectPoolMgr ObjectPoolMgr { get; private set; }
        internal AttributeInstanceMgr AttributeInstanceMgr { get; private set; }
        internal AbilityInstanceMgr AbilityInstanceMgr { get; private set; }
        internal GameUnitInstanceMgr GameUnitInstanceMgr { get; private set; }
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
            GameUnitInstanceMgr = new GameUnitInstanceMgr(this);
        }

        public override void UnInitSystem()
        {
            base.UnInitSystem();
            ObjectPoolMgr.Clear();
        }

        public override void UpdateSystem(float dt)
        {
            base.UpdateSystem(dt);
        }

        #region GameUnit

        public GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            return GameUnitInstanceMgr.CreateGameUnit(ref param);
        }

        public void DestroyGameUnit(GameUnit gameUnit)
        {
            GameUnitInstanceMgr.DestroyGameUnit(gameUnit);
        }

        #endregion

        public override void DumpObjectPool()
        {
            Debug.Log("----------Dump ObjectPools Start----------");
            Debug.Log("----------NodeObjectPool------------------");
            base.DumpObjectPool();
            Debug.Log("----------ObjectPool----------------------");
            ObjectPoolMgr.Log();
            Debug.Log("----------Dump ObjectPools End------------");
        }
        
    }
}