using System;
using System.Collections.Generic;
using GCL;
using GAS.Logic.Target;
using GAS.Logic.Value;
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
        public IValueProvider ValueProvider;
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
        
        //Value Provider
        internal IValueProvider ValueProvider { get; private set; }
        
        //Observable
        public readonly Observable<GameUnitCreateObserve> OnUnitCreated = new (); 
        public readonly Observable<GameUnitDestroyObserve> OnUnitDestroyed = new ();

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
            ValueProvider = param.ValueProvider;

            Assert.IsNotNull(AssetConfigProvider);
            Assert.IsNotNull(CommandDelegator);
            Assert.IsNotNull(TargetSearcher);
            Assert.IsNotNull(ValueProvider);
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
            EffectInstanceSubsystem = AddSubsystem<EffectInstanceSubsystem>(true);
            GameCueSubsystem = AddSubsystem<GameCueSubsystem>(false);
            AbilityActivationReqSubsystem = AddSubsystem<AbilityActivationReqSubsystem>(true);
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
        public void PostGameEvent(GameEventCreateParam param)
        {
            GameEventSubsystem.PostGameEvent(ref param);
        }
        
        #endregion
        
        #region GameUnit

        public GameUnit CreateGameUnit(ref GameUnitCreateParam param) => UnitInstanceSubsystem.CreateGameUnit(ref param);
        public void DestroyGameUnit(Handler<GameUnit> unitHandler) => UnitInstanceSubsystem.DestroyGameUnit(unitHandler);
        public void DestroyGameUnit(GameUnit unit) => UnitInstanceSubsystem.DestroyGameUnit(unit.Handler);
        public Handler<GameUnit>[] GetAllGameUnits(out uint nums) => UnitInstanceSubsystem.GetAllUnits(out nums);

        public static void RegisterOnAddTag(GameUnit unit, object observer, Action<EGameTag> callback, int priority = 0)
        {
            unit.OnAddTag.RegisterObserver(observer, callback, priority);
        }

        public static void UnregisterOnAddTag(GameUnit unit, object observer)
        {
            unit.OnAddTag.UnRegisterObserver(observer);
        }
        
        public static void RegisterOnRemoveTag(GameUnit unit, object observer, Action<EGameTag> callback, int priority = 0)
        {
            unit.OnRemoveTag.RegisterObserver(observer, callback, priority);
        }

        public static void UnregisterOnRemoveTag(GameUnit unit, object observer)
        {
            unit.OnRemoveTag.UnRegisterObserver(observer);
        }

        #endregion

        #region  Observe Attribute OnValChange

        public static void RegisterAttributeOnValChange(object observer, SimpleAttribute attribute, Action<AttributeChangeMsg> callback, int priority = 0)
        {
            attribute.OnValChanged.RegisterObserver(observer, callback, priority);
        }
        
        public static void UnRegisterAttributeOnValChange(object observer, SimpleAttribute attribute)
        {
            attribute.OnValChanged.UnRegisterObserver(observer);
        }

        public static void RegisterAttributeOnValChange(object observer, CompositeAttribute attribute, Action<AttributeChangeMsg> callback, int priority = 0)
        {
            attribute.OnValChanged.RegisterObserver(observer, callback, priority);
        }
        
        public static void UnRegisterAttributeOnValChange(object observer, CompositeAttribute attribute)
        {
            attribute.OnValChanged.UnRegisterObserver(observer);
        }
        
        public static void RegisterAttributeOnPlayValChangeCue(object observer, SimpleAttribute attribute, Action<AttributeChangeForCue> callback, int priority = 0)
        {
            attribute.OnPlayValChangeCue.RegisterObserver(observer, callback, priority);
        }
        
        public static void UnRegisterAttributeOnPlayValChangeCue(object observer, SimpleAttribute attribute)
        {
            attribute.OnPlayValChangeCue.UnRegisterObserver(observer);
        }
        public static void RegisterAttributeOnPlayValChangeCue(object observer, CompositeAttribute attribute, Action<AttributeChangeForCue> callback, int priority = 0)
        {
            attribute.OnPlayValChangeCue.RegisterObserver(observer, callback, priority);
        }
        
        public static void UnRegisterAttributeOnPlayValChangeCue(object observer, CompositeAttribute attribute)
        {
            attribute.OnPlayValChangeCue.UnRegisterObserver(observer);
        }

        public static void PlayAttributeOnChangeCue(SimpleAttribute attribute, ref AttributeChangeForCue changeForCue)
        {
            attribute.OnPlayValChangeCue.NotifyObservers(changeForCue);
        }
        
        public static void PlayAttributeOnChangeCue(CompositeAttribute attribute, ref AttributeChangeForCue changeForCue)
        {
            attribute.OnPlayValChangeCue.NotifyObservers(changeForCue);
        }
        
        #endregion

        #region Log

         public override void DumpObjectPool()
        {
            GameLogger.Log("----------Dump ObjectPools Start----------");
            GameLogger.Log("----------NodeObjectPool------------------");
            base.DumpObjectPool();
            GameLogger.Log("----------ObjectPool----------------------");
            ClassObjectPoolSubsystem.Log();
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