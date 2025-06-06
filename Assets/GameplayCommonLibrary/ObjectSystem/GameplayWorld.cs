using System;
using System.Collections.Generic;

namespace GameplayCommonLibrary
{
    /// <summary>
    /// 基础的ObjectModel: World --- System --- Entity --- Component
    /// </summary>
    public class GameplayWorld
    {
        private readonly Dictionary<Type, GameplayWorldSystem> _systems = new();
        private readonly List<GameplayWorldSystem> _orderedSystems = new(); //ordered
        private readonly List<GameplayWorldSystem> _tickableSystems = new(); //orderer
        private readonly List<GameplayWorldSystem> _systemRegisterTypeIsNotSelfTypeList = new();

        public IEntityMgr EntityMgr { get; private set; }

        public virtual void OnCreate()
        {
            EntityMgr = new DefaultEntityMgr();
            EntityMgr.OnCreate();
            CreateSystems();
        }

        public virtual void CreateSystems()
        {
            
        }
        
        public virtual void Init()
        {
            foreach (var sys in _orderedSystems)
            {
                sys.Init();
            }
        }

        public virtual void UnInit()
        {
            for (var i = _orderedSystems.Count; i >= 0; i--)
            {
                _orderedSystems[i].UnInit();
            }
        }
        
        public virtual void OnDestroy()
        {
            for (var i = _orderedSystems.Count; i >= 0; i--)
            {
                _orderedSystems[i].OnDestroy();
            }
            
            _systems.Clear();
            _tickableSystems.Clear();
            _orderedSystems.Clear();
            _systemRegisterTypeIsNotSelfTypeList.Clear();
        }

        public virtual void Update(float dt)
        {
            foreach (var system in _tickableSystems)
            {
                if(!system.Enabled)
                    continue;
                system.Update(dt);
            }
        }

        #region System
        
        public bool AddSystem(GameplayWorldSystem system)
        {
            if (_systems.ContainsKey(system.GetRegisterType()))
            {
                return false;
            }

            system.OnCreate(this);
            _systems.Add(system.GetRegisterType(), system);
            _orderedSystems.Add(system);
            _orderedSystems.Sort((x, y) => x.GetExecuteOrder() - y.GetExecuteOrder());

            if (system.IsTickable())
            {
                _tickableSystems.Add(system);
                _tickableSystems.Sort((x, y) => x.GetExecuteOrder() - y.GetExecuteOrder());
            }

            if (system.GetType() != system.GetRegisterType())
                _systemRegisterTypeIsNotSelfTypeList.Add(system);

            return true;
        }

        public GameplayWorldSystem GetSystem(Type systemType)
        {
            if (_systems.TryGetValue(systemType, out var system))
            {
                return system.Enabled ? system : null;
            }

            foreach (var sys in _systemRegisterTypeIsNotSelfTypeList)
            {
                if (sys.GetRegisterType() != systemType)
                    continue;
                system = sys;
                return system.Enabled ? system : null;
            }

            return null;
        }

        public T GetSystem<T>() where T : GameplayWorldSystem
        {
            if (_systems.TryGetValue(typeof(T), out var s))
            {
                var system = s as T;
                if (system?.Enabled ?? false)
                {
                    return system;
                }
                return null;
            }

            foreach (var sys in _systemRegisterTypeIsNotSelfTypeList)
            {
                if (sys.GetRegisterType() != typeof(T))
                    continue;
                var system = sys as T;
                if (system?.Enabled ?? false)
                {
                    return system;
                }
                return null;
            }

            return null;
        }

        #endregion
    }
}