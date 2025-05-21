using System;
using System.Collections.Generic;

namespace GameplayCommonLibrary
{
    /// <summary>
    /// 基础的ObjectModel: World --- System --- Entity --- Component
    /// </summary>
    public class GameplayWorld
    {
        private readonly Dictionary<Type, GameplayWorldSystem> _systems = new Dictionary<Type, GameplayWorldSystem>();
        private readonly List<GameplayWorldSystem> _tickableSystems = new();

        public virtual void Init()
        {
            
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
                system.Update(dt);
            }
        }
        
        #region Subsystem

        //Add顺序会影响Update顺序
        private T AddSystem<T>(bool isTickable) where T : GameplayWorldSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)))
            {
                GameLogger.LogError($"[GameplayWorld]System of {typeof(T)} already exists");
                return _systems[typeof(T)] as T;
            }
            var system = new T();
            system.OnCreate(this);
            _systems.Add(typeof(T), system);
            
            if(isTickable)
                _tickableSystems.Add(system);

            return system;
        }

        public T GetSubsystem<T>() where T : GameplayWorldSystem
        {
            return _systems.GetValueOrDefault(typeof(T)) as T;
        }

        #endregion
    }
}