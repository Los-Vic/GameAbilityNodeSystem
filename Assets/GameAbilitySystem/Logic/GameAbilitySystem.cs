﻿using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;
using NS;
using UnityEngine.Assertions;

namespace GAS.Logic
{
    public struct GameAbilitySystemCreateParam
    {
        public IAssetConfigProvider AssetConfigProvider;
        public ICommandDelegator CommandDelegator;
        public int PlayerNums;
    }

    public struct GameUnitCreateObserve
    {
        public ECreateUnitReason Reason;
        public GameUnit Unit;
    }
    
    public struct GameUnitDestroyObserve
    {
        public EDestroyUnitReason Reason;
        public GameUnit Unit;
    }
    
    public class GameAbilitySystem:NodeSystem
    {
        //Data
        internal int PlayerNums { get; private set; }
        internal FP DeltaTime { get; private set; }
        
        //Subsystem
        private readonly Dictionary<Type, GameAbilitySubsystem> _subsystems = new();
        private readonly List<GameAbilitySubsystem> _tickableSubsystems = new();
        
        //Provider
        internal IAssetConfigProvider AssetConfigProvider { get; private set; }
        
        //Command Delegator
        internal ICommandDelegator CommandDelegator { get; private set; }
        
        //Observable
        public readonly Observable<GameUnitCreateObserve> OnUnitCreated = new (); 
        public readonly Observable<GameUnitDestroyObserve> OnUnitDestroyed = new ();
        
        public GameAbilitySystem(GameAbilitySystemCreateParam param)
        {
            PlayerNums = param.PlayerNums;
            AssetConfigProvider = param.AssetConfigProvider;
            CommandDelegator = param.CommandDelegator;

            Assert.IsNotNull(AssetConfigProvider);
            Assert.IsNotNull(CommandDelegator);
        }

        public override void OnCreateSystem()
        {
            base.OnCreateSystem();
            
            AddSubsystem<ClassObjectPoolSubsystem>(false);
            AddSubsystem<GameEventSubsystem>(false);
            AddSubsystem<GameTagSubsystem>(false);
            AddSubsystem<AttributeInstanceSubsystem>(false);
            AddSubsystem<AbilityInstanceSubsystem>(true);
            AddSubsystem<UnitInstanceSubsystem>(false);
            AddSubsystem<EffectInstanceSubsystem>(false);
            AddSubsystem<GameCueSubsystem>(false);
            
            var abilityActivationReqSubsystem = AddSubsystem<AbilityActivationReqSubsystem>(true);
            abilityActivationReqSubsystem.CreatePlayerQueues(PlayerNums);
        }

        public override void InitSystem()
        {
            base.InitSystem();
            
            foreach (var subsystem in _subsystems.Values)
            {
                subsystem.Init();
            }
        }

        public override void UnInitSystem()
        {
            OnUnitCreated.Clear();
            OnUnitDestroyed.Clear();
            
            foreach (var subsystem in _subsystems.Values)
            {
                subsystem.UnInit();
            }
            
            base.UnInitSystem();
        }

        public override void UpdateSystem(float dt)
        {
            DeltaTime = dt;
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

        // public GameplayEvent<GameEventArg> GetGameEvent(EGameEventType type)
        // {
        //     return GetSubsystem<GameEventSubsystem>().GetGameEvent(type);
        // }

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

        public void GetAllGameUnits(ref List<GameUnit> unitList)
        {
            var units = GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.GetActiveObjects(typeof(GameUnit));
            unitList.Clear();
            
            if(units == null)
                return;
            foreach (var u in units)
            {
                if (u is GameUnit gameUnit)
                {
                    unitList.Add(gameUnit);
                }
            }
        }

        public GameUnit GetGameUnitByInstanceID(int instanceID)
        {
            var unitSubsystem = GetSubsystem<UnitInstanceSubsystem>();
            return unitSubsystem.GetGameUnitByInstanceID(instanceID);
        }

        #endregion

        #region GameAbility

        public GameAbility GetGameAbilityByInstanceID(int instanceID)
        {
            var subsystem = GetSubsystem<AbilityInstanceSubsystem>();
            return subsystem.GetAbilityByInstanceID(instanceID);
        }

        #endregion

        #region Log

         public override void DumpObjectPool()
        {
            GameLogger.Log("----------Dump ObjectPools Start----------");
            GameLogger.Log("----------NodeObjectPool------------------");
            base.DumpObjectPool();
            GameLogger.Log("----------ObjectPool----------------------");
            GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Log();
            GameLogger.Log("----------Dump ObjectPools End------------");
        }
        public void DumpRefCounterObjects()
        {
            var poolMgr = GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr;

            var listGameEvent = poolMgr.GetActiveObjects(typeof(GameEventArg));
            if (listGameEvent != null)
            {
                foreach (var arg in listGameEvent)
                {
                    var a = (GameEventArg)arg;
                    var logList = a.GetRefCountDisposableComponent().GetRefLog();
                    foreach (var log in logList)
                    {
                        GameLogger.Log(log);
                    }
                }
            }
            
            var listGameAbility = poolMgr.GetActiveObjects(typeof(GameAbility));
            if (listGameAbility != null)
            {
                foreach (var arg in listGameAbility)
                {
                    var a = (GameAbility)arg;
                    var logList = a.GetRefCountDisposableComponent().GetRefLog();
                    foreach (var log in logList)
                    {
                        GameLogger.Log(log);
                    }
                }
            }
            
            var listGameEffect = poolMgr.GetActiveObjects(typeof(GameEffect));
            if (listGameEffect != null)
            {
                foreach (var arg in listGameEffect)
                {
                    var a = (GameEffect)arg;
                    var logList = a.GetRefCountDisposableComponent().GetRefLog();
                    foreach (var log in logList)
                    {
                        GameLogger.Log(log);
                    }
                }
            }
        }

        #endregion

        #region Cue

        public void RegisterCueObservables(object obj, RegisterCueParam param)
        {
            GetSubsystem<GameCueSubsystem>().RegisterCueObservables(obj, param);
        }

        public void UnregisterCueObservables(object obj)
        {
            GetSubsystem<GameCueSubsystem>().UnregisterCueObservables(obj);
        }

        #endregion
    }
}