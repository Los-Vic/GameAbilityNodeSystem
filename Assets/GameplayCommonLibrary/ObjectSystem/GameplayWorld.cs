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
        private readonly List<GameplayWorldSystem> _tickableSystems = new();
        private readonly List<GameplayWorldSystem> _systemRegisterTypeIsNotSelfTypeList = new();

        public IEntityMgr EntityMgr { get; private set; }

        public virtual void OnCreate()
        {
            EntityMgr = new DefaultEntityMgr();
        }

        public virtual void Init()
        {
            foreach (var system in _systems.Values)
            {
                system.Init();
            }
        }

        public virtual void UnInit()
        {
            foreach (var system in _systems.Values)
            {
                system.UnInit();
            }

            _systems.Clear();
            _tickableSystems.Clear();
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

        //Add顺序会影响Update顺序
        public bool AddSystem(GameplayWorldSystem system)
        {
            if (_systems.ContainsKey(system.GetRegisterType()))
            {
                return false;
            }

            system.OnCreate(this);
            _systems.Add(system.GetRegisterType(), system);

            if (system.IsTickable())
                _tickableSystems.Add(system);

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
            if (_systems.ContainsKey(typeof(T)))
            {
                var system = _systems[typeof(T)] as T;
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