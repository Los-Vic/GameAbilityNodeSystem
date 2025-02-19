using System;
using System.Collections.Generic;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    public class GameAbilitySystem:NodeSystem
    {
        //Subsystem
        private readonly Dictionary<Type, GameAbilitySubsystem> _subsystems = new();
        private readonly List<GameAbilitySubsystem> _tickableSubsystems = new();
        
        //Provider
        internal IAssetConfigProvider AssetConfigProvider { get; private set; }
        
        public GameAbilitySystem(IAssetConfigProvider provider)
        {
            AssetConfigProvider = provider;
        }
        
        public override void InitSystem()
        {
            base.InitSystem();
            
            CreateSubsystem<ObjectPoolSubsystem>(false);
            CreateSubsystem<GameEventSubsystem>(false);
            CreateSubsystem<AttributeInstanceSubsystem>(false);
            CreateSubsystem<AbilityInstanceSubsystem>(false);
            CreateSubsystem<UnitInstanceSubsystem>(false);
            CreateSubsystem<AbilityActivationReqSubsystem>(false);
        }

        public override void UnInitSystem()
        {
            foreach (GameAbilitySubsystem subsystem in _subsystems.Values)
            {
                subsystem.UnInit();
            }
            _subsystems.Clear();
            _tickableSubsystems.Clear();
            
            base.UnInitSystem();
        }

        public override void ResetSystem()
        {
            foreach (GameAbilitySubsystem subsystem in _subsystems.Values)
            {
                subsystem.Reset();
            }
            base.ResetSystem();
        }

        public override void UpdateSystem(float dt)
        {
            base.UpdateSystem(dt);
            foreach (var subsystem in _tickableSubsystems)
            {
                subsystem.Update(dt);
            }
        }

        #region Subsystem

        private void CreateSubsystem<T>(bool isTickable) where T : GameAbilitySubsystem, new()
        {
            if (_subsystems.ContainsKey(typeof(T)))
            {
                Logger.LogError($"Subsystem of {typeof(T)} already exists");
                return;
            }
            var subsystem = new T();
            subsystem.Init(this);
            _subsystems.Add(typeof(T), subsystem);
            
            if(isTickable)
                _tickableSubsystems.Add(subsystem);
        }

        public T GetSubsystem<T>() where T : GameAbilitySubsystem
        {
            return _subsystems.GetValueOrDefault(typeof(T)) as T;
        }

        #endregion
        
        
        #region GameUnit

        public GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            return GetSubsystem<UnitInstanceSubsystem>().CreateGameUnit(ref param);
        }

        public void DestroyGameUnit(GameUnit gameUnit)
        {
            GetSubsystem<UnitInstanceSubsystem>().DestroyGameUnit(gameUnit);
        }

        #endregion

        public override void DumpObjectPool()
        {
            Logger?.Log("----------Dump ObjectPools Start----------");
            Logger?.Log("----------NodeObjectPool------------------");
            base.DumpObjectPool();
            Logger?.Log("----------ObjectPool----------------------");
            GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Log();
            Logger?.Log("----------Dump ObjectPools End------------");
        }
        
    }
}