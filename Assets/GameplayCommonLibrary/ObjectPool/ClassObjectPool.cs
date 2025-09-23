using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GCL
{
    public interface IPoolObject
    {
        void OnCreateFromPool();
        void OnTakeFromPool();
        void OnReturnToPool();
        void OnDestroy();
    }


    public class ClassObjectPool
    {
        //Unity Pool里只会保留Inactive对象的引用， 只有当Inactive的对象超过maxsize才会触发destroy
        private readonly UnityEngine.Pool.ObjectPool<IPoolObject> _pool;
        private readonly Type _poolObjectType;

        //Constructor
        public ClassObjectPool(Type type, int capacity, int maxSize)
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
            obj.OnTakeFromPool();
            return obj;
        }

        public void Release(IPoolObject obj)
        {
            if (_poolObjectType != obj.GetType())
            {
                GameLogger.LogError($"[ObjectPool]destroy object failed: pool type [{_poolObjectType}] is not equal to type [{obj.GetType()}]");
                return;
            }
            obj.OnReturnToPool();
            _pool.Release(obj);
        }
        
        public void Clear()
        {
            GameLogger.Log($"[ObjectPool]clear object pool, type [{_poolObjectType}], active [{_pool.CountActive}], total [{_pool.CountAll}]");
            _pool.Clear();
        }
        
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
            GameLogger.Log(
                $"[ObjectPool][{_poolObjectType.Name}]: active/total:[{_pool.CountActive}/{_pool.CountAll}]");
            return (_pool.CountActive, _pool.CountAll);
        }

        #endregion
    }
    
    public class ClassObjectPoolCollection
    {
        private readonly Dictionary<Type, ClassObjectPool> _objectPoolMap = new();
        private const int DefaultCapacity = 32;
        private const int DefaultMaxSize = 10000;

        public T Get<T>() where T : class, IPoolObject, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                pool = new ClassObjectPool(typeof(T), DefaultCapacity, DefaultMaxSize);
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
                pool = new ClassObjectPool(type, DefaultCapacity, DefaultMaxSize);
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
        
        public IEnumerable<ClassObjectPool> GetAllPools()
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

    public static class ClassObjectPoolMgr
    {
        private static ConcurrentDictionary<int, ClassObjectPoolCollection> _objectPoolCollections = new();
        
        public static ClassObjectPoolCollection Get(int worldId)
        {
            if (_objectPoolCollections.TryGetValue(worldId, out var poolCollection)) 
                return poolCollection;
            
            poolCollection = new ClassObjectPoolCollection();
            _objectPoolCollections.TryAdd(worldId, poolCollection);
            return poolCollection;
        }

        public static void Release(int worldId)
        {
            _objectPoolCollections.TryRemove(worldId, out var poolCollection);
        }
    }
}