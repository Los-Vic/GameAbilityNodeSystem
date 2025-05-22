using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace GameplayCommonLibrary
{
    public class DefaultEntityMgr:IEntityMgr
    {
        private uint _entityIDCounter;
        private readonly Dictionary<uint, GameplayWorldEntity> _entityInstanceLookUp = new();

        private ObjectPool<GameplayWorldEntity> _entityPool;
        private readonly Dictionary<Type, ObjectPool<GameplayWorldComponent>> _componentPools = new();

        private const int EntityPoolMaxSize = 10000;
        private const int ComponentPoolMaxSize = 10000;

        public void OnCreate()
        {
            _entityPool = new ObjectPool<GameplayWorldEntity>(OnCreateEntity, null, null,
                null, true, 100, EntityPoolMaxSize);
        }

        public void Init()
        {
          
        }

        public void UnInit()
        {
            _entityIDCounter = 0;
            _entityInstanceLookUp.Clear();
            _entityPool.Clear();
            foreach (var pool in _componentPools.Values)
            {
                pool.Clear();
            }
            _componentPools.Clear();
        }
        
        #region Entity

        public GameplayWorldEntity CreateEntity()
        {
            _entityIDCounter++;
            var entity = _entityPool.Get();
            entity.SetEntityID(_entityIDCounter);
            _entityInstanceLookUp.Add(entity.EntityID, entity);
            return entity;
        }

        public void DestroyEntity(GameplayWorldEntity entity)
        {
            var components = entity.GetComponents();
            foreach (var component in components)
            {
                component.OnRemove();
            }
            entity.ClearComponents();
            _entityInstanceLookUp.Remove(entity.EntityID);
            _entityPool.Release(entity);
        }

        public GameplayWorldEntity GetEntity(uint entityID)
        {
            return _entityInstanceLookUp.GetValueOrDefault(entityID);
        }

        #endregion

        #region Component

        public T CreateComponent<T>() where T : GameplayWorldComponent, new()
        {
            return CreateComponent(typeof(T)) as T;
        }

        public GameplayWorldComponent CreateComponent(Type componentType)
        {
            if (!_componentPools.TryGetValue(componentType, out var pool))
            {
                pool = new ObjectPool<GameplayWorldComponent>(() => OnCreateComponent(componentType), null, null, null,
                    true, 100, ComponentPoolMaxSize);
                _componentPools.Add(componentType, pool);
            }
            return pool.Get();
        }
        
        public void DestroyComponent(GameplayWorldComponent component)
        {
            if (!_componentPools.TryGetValue(component.GetType(), out var pool))
                return;
            pool.Release(component);
        }
        

        #endregion

        #region Object Pool
        private static GameplayWorldEntity OnCreateEntity()
        {
            return new GameplayWorldEntity();
        }

        private static GameplayWorldComponent OnCreateComponent(Type componentType)
        {
            if (!componentType.IsSubclassOf(typeof(GameplayWorldComponent)))
                return null;
            return Activator.CreateInstance(componentType) as GameplayWorldComponent;
        }

        #endregion
       
    }
}