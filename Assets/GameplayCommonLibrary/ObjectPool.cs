using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayCommonLibrary
{
    public interface IPoolObject
    {
        void OnCreateFromPool(ObjectPool pool);
        void OnTakeFromPool();
        void OnReturnToPool();
        void OnDestroy();
    }


    public class ObjectPool
    {
        //Unity Pool里只会保留Inactive对象的引用， 只有当Inactive的对象超过maxsize才会触发destroy
        private readonly UnityEngine.Pool.ObjectPool<IPoolObject> _pool;
        private readonly Type _poolObjectType;
        private readonly List<IPoolObject> _activePoolObjects = new();

        //Constructor
        public ObjectPool(Type type, int capacity, int maxSize)
        {
            var t = typeof(IPoolObject);
            if (!t.IsAssignableFrom(type))
            {
                GameLogger.LogError(
                    $"[ObjectPool]create object pool failed: type [{type}] can not assign to IPoolObject");
                return;
            }

            GameLogger.Log($"[ObjectPool]create object pool success: type [{type}]");
            _poolObjectType = type;
            _pool = new UnityEngine.Pool.ObjectPool<IPoolObject>(CreateItem, OnTakeFromPool, OnReturnToPool,
                OnDestroyItem,
                true, capacity, maxSize);
        }

        public IPoolObject Get()
        {
            var obj = _pool.Get();
            _activePoolObjects.Add(obj);
            obj.OnTakeFromPool();
            return obj;
        }

        public void Release(IPoolObject obj)
        {
            if (_activePoolObjects.Remove(obj))
            {
                obj.OnReturnToPool();
                _pool.Release(obj);
            }
            else
            {
                if (_poolObjectType != obj.GetType())
                {
                    GameLogger.LogError($"[ObjectPool]destroy object failed: pool type [{_poolObjectType}] is not equal to type [{obj.GetType()}]");
                }
                else
                {
                    GameLogger.LogError($"[ObjectPool]destroy object failed: object is already destroyed");
                }
            }
           
        }
        
        public void Clear()
        {
            GameLogger.Log($"[ObjectPool]clear object pool, type [{_poolObjectType}], active [{_activePoolObjects.Count}], total [{_pool.CountAll}]");
            _pool.Clear();
            
            foreach (var obj in _activePoolObjects)
            {
                obj.OnDestroy(); 
            }
            _activePoolObjects.Clear();
        }
        
        public List<IPoolObject> GetActiveObjects() => _activePoolObjects;
        public bool IsActiveObject(IPoolObject obj) => _activePoolObjects.Contains(obj);

        
        #region Pool Callback

        private IPoolObject CreateItem()
        {
            var obj = Activator.CreateInstance(_poolObjectType) as IPoolObject;

            if (obj == null)
            {
                GameLogger.LogError(
                    $"[ObjectPool]create object item failed: type [{_poolObjectType}] can not assign to IPoolObject");
                return null;
            }
           
            obj.OnCreateFromPool(this);
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
            GameLogger.Log(
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

        public T Get<T>() where T : class, IPoolObject, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                pool = new ObjectPool(typeof(T), DefaultCapacity, DefaultMaxSize);
                _objectPoolMap.Add(type, pool);
            }

            return pool.Get() as T;
        }

        public IPoolObject Get(Type type)
        {
            var t = typeof(IPoolObject);
            if (!t.IsAssignableFrom(type))
            {
                GameLogger.LogError($"create object failed: type {type} can not assign to IPoolObject");
                return null;
            }

            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                pool = new ObjectPool(type, DefaultCapacity, DefaultMaxSize);
                _objectPoolMap.Add(type, pool);
            }

            return pool.Get();
        }

        public void Release(IPoolObject obj)
        {
            var type = obj.GetType();
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return;

            pool.Release(obj);
        }
        
        public List<IPoolObject> GetActiveObjects(Type type)
        {
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return null;
            return pool.GetActiveObjects();
        }

        public bool IsActiveObject(IPoolObject obj)
        {
            var type = obj.GetType();
            return _objectPoolMap.TryGetValue(type, out var pool) && pool.IsActiveObject(obj);
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