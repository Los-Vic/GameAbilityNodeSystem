using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonObjectPool
{
    public interface IPoolObject
    {
        void OnCreateFromPool();
        void OnTakeFromPool();
        void OnReturnToPool();
        void OnDestroy();
    }


    public class ObjectPool
    {
        private readonly UnityEngine.Pool.ObjectPool<IPoolObject> _pool;
        private readonly Type _poolObjectType;
        private readonly List<IPoolObject> _poolObjects = new();

        //Constructor
        public ObjectPool(Type type, int capacity, int maxSize)
        {
            var t = typeof(IPoolObject);
            if (!t.IsAssignableFrom(type))
            {
                Debug.LogError(
                    $"[ObjectPool]create object pool failed: type [{type}] can not assign to IPoolObject");
                return;
            }

            Debug.Log($"[ObjectPool]create object pool success: type [{type}]");
            _poolObjectType = type;
            _pool = new UnityEngine.Pool.ObjectPool<IPoolObject>(CreateItem, OnTakeFromPool, OnReturnToPool,
                OnDestroyItem,
                true, capacity, maxSize);
        }

        public IPoolObject CreateObject()
        {
            var obj = _pool.Get();
            _poolObjects.Add(obj);
            obj.OnTakeFromPool();
            return obj;
        }

        public void DestroyObject(IPoolObject obj)
        {
            _poolObjects.Remove(obj);
            obj.OnReturnToPool();
            _pool.Release(obj);
        }
        
        public List<IPoolObject> GetObjects() => _poolObjects;

        public void Clear()
        {
            Debug.Log($"[ObjectPool]clear object pool, type [{_poolObjectType}], count [{_poolObjects.Count}]");
            _pool.Clear();
        }
        
        #region Pool Callback

        private IPoolObject CreateItem()
        {
            var obj = Activator.CreateInstance(_poolObjectType) as IPoolObject;
            obj.OnCreateFromPool();
            return obj;
        }

        private static void OnTakeFromPool(IPoolObject obj)
        {
        }

        private static void OnReturnToPool(IPoolObject obj)
        {
        }

        private static void OnDestroyItem(IPoolObject obj)
        {
            obj.OnDestroy();
        }

        #endregion


        #region Interface

        public (int, int) LogState()
        {
            Debug.Log(
                $"[ObjectPool][{_poolObjectType.Name}]: active/total:[{_pool.CountActive}/{_pool.CountAll}]");
            return (_pool.CountActive, _pool.CountAll);
        }

        #endregion
    }

    /// <summary>
    /// 对象池管理器，对象池实例都在这里
    /// </summary>
    public class ObjectPoolMgr
    {
        private readonly Dictionary<Type, ObjectPool> _objectPoolMap = new();
        private const int DefaultCapacity = 32;
        private const int DefaultMaxSize = 10000;

        public T CreateObject<T>() where T : class, IPoolObject, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                pool = new ObjectPool(typeof(T), DefaultCapacity, DefaultMaxSize);
                _objectPoolMap.Add(type, pool);
            }

            return pool.CreateObject() as T;
        }

        public IPoolObject CreateObject(Type type)
        {
            var t = typeof(IPoolObject);
            if (!t.IsAssignableFrom(type))
            {
                Debug.LogError($"create object failed: type {type} can not assign to IPoolObject");
                return default;
            }

            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                pool = new ObjectPool(type, DefaultCapacity, DefaultMaxSize);
                _objectPoolMap.Add(type, pool);
            }

            return pool.CreateObject();
        }

        public void DestroyObject(IPoolObject obj)
        {
            var type = obj.GetType();
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return;

            pool.DestroyObject(obj);
        }

        public List<T> GetObjects<T>() where T : class, IPoolObject
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return null;
            return pool.GetObjects() as List<T>;
        }
        
        public IEnumerable<ObjectPool> GetAllPools()
        {
            return _objectPoolMap.Values;
        }

        public void Clear()
        {
            foreach (var pool in _objectPoolMap.Values)
            {
                pool.Clear();
            }
            _objectPoolMap.Clear();
        }

        public void ClearPool(Type type)
        {
            if (_objectPoolMap.TryGetValue(type, out var pool))
            {
                pool.Clear();
            }
        }
        
        public void Log()
        {
            foreach (var pool in _objectPoolMap.Values)
            {
                pool.LogState();
            }
        }
    }
}