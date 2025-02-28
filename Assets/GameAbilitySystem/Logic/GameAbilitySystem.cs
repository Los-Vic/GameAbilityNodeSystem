using System;
using System.Collections.Generic;
using System.Reflection;
using GameplayCommonLibrary;
using NS;

namespace GAS.Logic
{
    public struct GameAbilitySystemCreateParam
    {
        public IAssetConfigProvider AssetConfigProvider;
        public int PlayerNums;
    }
    
    public class GameAbilitySystem:NodeSystem
    {
        //Data
        internal int PlayerNums { get; private set; }
        
        
        //Subsystem
        private readonly Dictionary<Type, GameAbilitySubsystem> _subsystems = new();
        private readonly List<GameAbilitySubsystem> _tickableSubsystems = new();
        
        //Provider
        internal IAssetConfigProvider AssetConfigProvider { get; private set; }
        
        public GameAbilitySystem(GameAbilitySystemCreateParam param)
        {
            PlayerNums = param.PlayerNums;
            AssetConfigProvider = param.AssetConfigProvider;
        }

        public override void OnCreateSystem()
        {
            base.OnCreateSystem();
            
            AddSubsystem<ObjectPoolSubsystem>(false);
            AddSubsystem<GameEventSubsystem>(false);
            AddSubsystem<AttributeInstanceSubsystem>(false);
            AddSubsystem<AbilityInstanceSubsystem>(true);
            AddSubsystem<UnitInstanceSubsystem>(false);
            
            var abilityActivationReqSubsystem = AddSubsystem<AbilityActivationReqSubsystem>(true);
            abilityActivationReqSubsystem.CreatePlayerQueues(PlayerNums);
        }

        public override void InitSystem()
        {
            base.InitSystem();

            foreach (GameAbilitySubsystem subsystem in _subsystems.Values)
            {
                subsystem.Init();
            }
        }

        public override void UnInitSystem()
        {
            foreach (GameAbilitySubsystem subsystem in _subsystems.Values)
            {
                subsystem.UnInit();
            }
            
            base.UnInitSystem();
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

        private T AddSubsystem<T>(bool isTickable) where T : GameAbilitySubsystem, new()
        {
            if (_subsystems.ContainsKey(typeof(T)))
            {
                GameLogger.LogError($"Subsystem of {typeof(T)} already exists");
                return _subsystems[typeof(T)] as T;
            }
            var subsystem = new T();
            subsystem.OnCreate(this);
            _subsystems.Add(typeof(T), subsystem);
            
            if(isTickable)
                _tickableSubsystems.Add(subsystem);

            return subsystem;
        }

        public T GetSubsystem<T>() where T : GameAbilitySubsystem
        {
            return _subsystems.GetValueOrDefault(typeof(T)) as T;
        }

        #endregion

        #region GameEvent

        public GameplayEvent<GameEventArg> GetGameEvent(EGameEventType type)
        {
            return GetSubsystem<GameEventSubsystem>().GetGameEvent(type);
        }

        public void PostGameEvent(GameEventInitParam param)
        {
            GetSubsystem<GameEventSubsystem>().PostGameEvent(ref param);
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
            GameLogger.Log("----------Dump ObjectPools Start----------");
            GameLogger.Log("----------NodeObjectPool------------------");
            base.DumpObjectPool();
            GameLogger.Log("----------ObjectPool----------------------");
            GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Log();
            GameLogger.Log("----------Dump ObjectPools End------------");
        }

        public void DumpRefCounterObjects()
        {
            var poolMgr = GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr;

            var list = poolMgr.GetActiveObjects(typeof(GameEventArg));
            if (list != null)
            {
                foreach (var arg in list)
                {
                    var a = (GameEventArg)arg;
                    var logList = a.GetRefCountDisposableComponent().GetRefLog();
                    foreach (var log in logList)
                    {
                        GameLogger.Log(log);
                    }
                }
            }
            
        }
    }
}