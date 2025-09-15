using System;
using System.Collections.Generic;
using System.Reflection;

namespace GCL
{
    /// <summary>
    /// 基础的ObjectModel: World --- System --- Entity --- Component
    /// </summary>
    public class GameplayWorld
    {
        private readonly Dictionary<Type, GameplayWorldSystem> _systems = new();
        private readonly List<GameplayWorldSystem> _orderedSystems = new(); //ordered
        private readonly List<GameplayWorldTickableSystem> _tickableSystems = new(); //orderer

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
            var registerType = system.GetType();
            
            var sysAttr = registerType.GetCustomAttribute<SystemAttribute>();
            if (sysAttr is { Parent: not null })
            {
                registerType = sysAttr.Parent;
            }
            
            if (_systems.ContainsKey(registerType))
            {
                return false;
            }

            system.OnCreate(this);
            _systems.Add(registerType, system);
            _orderedSystems.Add(system);
            _orderedSystems.Sort((x, y) => x.GetExecuteOrder() - y.GetExecuteOrder());

            if (system is GameplayWorldTickableSystem tickableSystem)
            {
                _tickableSystems.Add(tickableSystem);
                _tickableSystems.Sort((x, y) => x.GetExecuteOrder() - y.GetExecuteOrder());
            }

            return true;
        }

        //ReSharper restore Unity.ExpensiveCode
        public GameplayWorldSystem GetSystem(Type systemType)
        {
            var registerType = systemType;
            
            var sysAttr = registerType.GetCustomAttribute<SystemAttribute>();
            if (sysAttr is { Parent: not null })
            {
                registerType = sysAttr.Parent;
            }
            
            if (_systems.TryGetValue(registerType, out var system))
            {
                return system.Enabled ? system : null;
            }
            return null;
        }

        //ReSharper restore Unity.ExpensiveCode
        public T GetSystem<T>() where T : GameplayWorldSystem
        {
            var sys = GetSystem(typeof(T));
            return sys as T;
        }

        #endregion
    }
}