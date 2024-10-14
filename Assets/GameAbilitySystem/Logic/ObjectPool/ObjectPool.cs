using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameAbilitySystem.Logic.ObjectPool
{
    public struct ObjectPoolParam
    {
        public int Capacity;
        public int MaxSize;
    }
    
    public interface IPoolObject
    {
        ObjectPoolParam GetPoolParam();
        void OnCreateFromPool();
        void OnTakeFromPool();
        void OnReturnToPool();
        void OnDestroy();
    }

    public interface IObjectPool
    {
        (int, int) LogState();
    }

    public class ObjectPool<T> : IObjectPool where T : class, IPoolObject, new()
    {
        private readonly UnityEngine.Pool.ObjectPool<T> _pool;
        private readonly List<T> _activeObjects = new List<T>();
        
        //Constructor
        public ObjectPool(ObjectPoolParam param)
        {
            _pool = new UnityEngine.Pool.ObjectPool<T>(CreateItem, OnTakeFromPool, OnReturnToPool, OnDestroyItem,
                true, param.Capacity, param.MaxSize);
        }
        
        public T CreateObject()
        {
            var obj = _pool.Get();
            _activeObjects.Add(obj);
            obj.OnTakeFromPool();
            return obj;
        }

        public void DestroyObject(T obj)
        {
            if(!_activeObjects.Remove(obj))
                return;
            
            obj.OnReturnToPool();
            _pool.Release(obj);
        }
        
        #region Pool Callback
        private T CreateItem()
        {
            var obj = new T();
            obj.OnCreateFromPool();
            return obj;
        }

        private static void OnTakeFromPool(T obj)
        {
        }

        private static void OnReturnToPool(T obj)
        {
        }

        private static void OnDestroyItem(T obj)
        { 
            //this.LogWarning($"[EF]ObjectPool[{typeof(T).Name}] reach max size! consider increasing max size or decreasing the number of instances!");
            obj.OnDestroy();
        }
        #endregion


        #region Interface

        public (int, int) LogState()
        {
           Debug.Log($"ObjectPool[{typeof(T).Name}]: Active/Total/PoolActive:[{_activeObjects.Count}/{_pool.CountAll}/{_pool.CountActive}]");
           if (_activeObjects.Count != _pool.CountActive)
           {
               Debug.LogError($"ObjectPool[{typeof(T).Name}] has different active count: Active/Total/PoolActive:[{_activeObjects.Count}/{_pool.CountAll}/{_pool.CountActive}]");
           }
           return (_activeObjects.Count, _pool.CountAll);
        }

        #endregion
    }
    
    /// <summary>
    /// 对象池管理器，对象池实例都在这里
    /// </summary>
    public class ObjectPoolMgr
    {
        private readonly Dictionary<Type, IObjectPool> _objectPoolMap = new();
        public T CreateObject<T>() where T:class, IPoolObject, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                var defaultObj = new T();
                pool = new ObjectPool<T>(defaultObj.GetPoolParam());
                _objectPoolMap.Add(type, pool);
            }

            var p = (ObjectPool<T>)pool;
            return p.CreateObject();
        }

        public void DestroyObject<T>(T obj) where T:class, IPoolObject, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return;
            
            var p = (ObjectPool<T>)pool;
            p.DestroyObject(obj);
        }

        public IEnumerable<IObjectPool> GetAllPools()
        {
            return _objectPoolMap.Values;
        }
    }
}