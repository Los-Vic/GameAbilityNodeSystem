using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
using GAS.Logic.Target;
using MissQ;
using NS;
using UnityEngine.Assertions;

namespace GAS.Logic
{
    public struct GameAbilitySystemCreateParam
    {
        public IAssetConfigProvider AssetConfigProvider;
        public ICommandDelegator CommandDelegator;
        public ITargetSearcher  TargetSearcher;
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
        
        //Asset Provider
        internal IAssetConfigProvider AssetConfigProvider { get; private set; }
        
        //Command Delegator
        internal ICommandDelegator CommandDelegator { get; private set; }
        
        //Target Searcher
        internal ITargetSearcher TargetSearcher { get; private set; }
        
        //Observable
        public readonly Observable<GameUnitCreateObserve> OnUnitCreated = new (); 
        public readonly Observable<GameUnitDestroyObserve> OnUnitDestroyed = new ();
        
        //GameUnit
        private readonly List<GameUnit> _allGameUnits = new();

        #region SubSystems

        public ClassObjectPoolSubsystem ClassObjectPoolSubsystem { get; private set; }
        public GameEventSubsystem GameEventSubsystem {get; private set;}
        public GameTagSubsystem GameTagSubsystem { get; private set; }
        public AttributeInstanceSubsystem AttributeInstanceSubsystem { get; private set; }
        public AbilityInstanceSubsystem AbilityInstanceSubsystem { get;private set; }
        public UnitInstanceSubsystem UnitInstanceSubsystem { get; private set; }
        public EffectInstanceSubsystem EffectInstanceSubsystem { get; private set; }
        public GameCueSubsystem GameCueSubsystem { get;private set; }
        public AbilityActivationReqSubsystem AbilityActivationReqSubsystem { get; private set; }

        #endregion
        
        
        public GameAbilitySystem(GameAbilitySystemCreateParam param)
        {
            PlayerNums = param.PlayerNums;
            AssetConfigProvider = param.AssetConfigProvider;
            CommandDelegator = param.CommandDelegator;
            TargetSearcher = param.TargetSearcher;

            Assert.IsNotNull(AssetConfigProvider);
            Assert.IsNotNull(CommandDelegator);
        }

        public override void OnCreateSystem()
        {
            base.OnCreateSystem();
            
            ClassObjectPoolSubsystem = AddSubsystem<ClassObjectPoolSubsystem>(false);
            GameEventSubsystem = AddSubsystem<GameEventSubsystem>(true);
            GameTagSubsystem = AddSubsystem<GameTagSubsystem>(false);
            AttributeInstanceSubsystem = AddSubsystem<AttributeInstanceSubsystem>(false);
            AbilityInstanceSubsystem = AddSubsystem<AbilityInstanceSubsystem>(true);
            UnitInstanceSubsystem = AddSubsystem<UnitInstanceSubsystem>(false);
            EffectInstanceSubsystem = AddSubsystem<EffectInstanceSubsystem>(false);
            GameCueSubsystem = AddSubsystem<GameCueSubsystem>(false);
            
            AbilityActivationReqSubsystem = AddSubsystem<AbilityActivationReqSubsystem>(true);
            AbilityActivationReqSubsystem.CreatePlayerQueues(PlayerNums);
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
        
        #endregion

        #region GameEvent

        // public GameplayEvent<GameEventArg> GetGameEvent(EGameEventType type)
        // {
        //     return GetSubsystem<GameEventSubsystem>().GetGameEvent(type);
        // }

        public void PostGameEvent(GameEventInitParam param)
        {
            GameEventSubsystem.PostGameEvent(ref param);
        }
        

        #endregion
        
        #region GameUnit

        public GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var unit = UnitInstanceSubsystem.CreateGameUnit(ref param);
            _allGameUnits.Add(unit);
            return unit;
        }

        public void DestroyGameUnit(GameUnit gameUnit)
        {
            _allGameUnits.Remove(gameUnit);
            UnitInstanceSubsystem.DestroyGameUnit(gameUnit);
        }

        public void GetAllGameUnits(ref List<GameUnit> unitList)
        {
            unitList.Clear();
            foreach (var u in _allGameUnits)
            {
                unitList.Add(u);
            }
        }

        public GameUnit GetGameUnitByInstanceID(int instanceID)
        {
            return UnitInstanceSubsystem.GetGameUnitByInstanceID(instanceID);
        }

        #endregion

        #region GameAbility

        public GameAbility GetGameAbilityByInstanceID(int instanceID)
        {
            return AbilityInstanceSubsystem.GetAbilityByInstanceID(instanceID);
        }

        #endregion

        #region Log

         public override void DumpObjectPool()
        {
            GameLogger.Log("----------Dump ObjectPools Start----------");
            GameLogger.Log("----------NodeObjectPool------------------");
            base.DumpObjectPool();
            GameLogger.Log("----------ObjectPool----------------------");
            ClassObjectPoolSubsystem.ClassObjectPoolMgr.Log();
            GameLogger.Log("----------Dump ObjectPools End------------");
        }

        #endregion

        #region Cue

        public void RegisterCueObservables(object obj, RegisterCueParam param)
        {
            GameCueSubsystem.RegisterCueObservables(obj, ref param);
        }

        public void UnregisterCueObservables(object obj)
        {
            GameCueSubsystem.UnregisterCueObservables(obj);
        }

        #endregion
    }
}