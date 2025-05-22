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
        private bool AddSystem(GameplayWorldSystem system)
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

        public bool GetSystem(Type systemType, out GameplayWorldSystem system)
        {
            if (_systems.TryGetValue(systemType, out system))
            {
                return system.Enabled;
            }

            foreach (var sys in _systemRegisterTypeIsNotSelfTypeList)
            {
                if (sys.GetRegisterType() != systemType)
                    continue;
                system = sys;
                return system.Enabled;
            }

            return false;
        }

        public bool GetSystem<T>(out T system) where T : GameplayWorldSystem
        {
            system = null;
            if (_systems.ContainsKey(typeof(T)))
            {
                system = _systems[typeof(T)] as T;
                return system?.Enabled ?? false;
            }

            foreach (var sys in _systemRegisterTypeIsNotSelfTypeList)
            {
                if (sys.GetRegisterType() != typeof(T))
                    continue;
                system = sys as T;
                return system?.Enabled ?? false;
            }

            return false;
        }

        #endregion
    }
}