using System;
using System.Collections.Generic;

namespace GameplayCommonLibrary
{
    public interface IPoolClass
    {
        void OnCreateFromPool(ClassObjectPool pool);
        void OnTakeFromPool();
        void OnReturnToPool();
        void OnDestroy();
    }


    public class ClassObjectPool
    {
        //Unity Pool里只会保留Inactive对象的引用， 只有当Inactive的对象超过maxsize才会触发destroy
        private readonly UnityEngine.Pool.ObjectPool<IPoolClass> _pool;
        private readonly Type _poolObjectType;
        private readonly List<IPoolClass> _activePoolObjects = new();

        //Constructor
        public ClassObjectPool(Type type, int capacity, int maxSize)
        {
            var t = typeof(IPoolClass);
            if (!t.IsAssignableFrom(type))
            {
                GameLogger.LogError(
                    $"[ObjectPool]create object pool failed: type [{type}] can not assign to IPoolObject");
                return;
            }

            GameLogger.Log($"[ObjectPool]create object pool success: type [{type}]");
            _poolObjectType = type;
            _pool = new UnityEngine.Pool.ObjectPool<IPoolClass>(CreateItem, OnTakeFromPool, OnReturnToPool,
                OnDestroyItem,
                true, capacity, maxSize);
        }

        public IPoolClass Get()
        {
            var obj = _pool.Get();
            _activePoolObjects.Add(obj);
            obj.OnTakeFromPool();
            return obj;
        }

        public void Release(IPoolClass obj)
        {
            if (_poolObjectType != obj.GetType())
            {
                GameLogger.LogError($"[ObjectPool]destroy object failed: pool type [{_poolObjectType}] is not equal to type [{obj.GetType()}]");
                return;
            }
            
            if (_activePoolObjects.Remove(obj))
            {
                obj.OnReturnToPool();
                _pool.Release(obj);
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
        
        public List<IPoolClass> GetActiveObjects() => _activePoolObjects;
        public bool IsActiveObject(IPoolClass obj) => _activePoolObjects.Contains(obj);

        
        #region Pool Callback

        private IPoolClass CreateItem()
        {
            var obj = Activator.CreateInstance(_poolObjectType) as IPoolClass;

            if (obj == null)
            {
                GameLogger.LogError(
                    $"[ObjectPool]create object item failed: type [{_poolObjectType}] can not assign to IPoolObject");
                return null;
            }
           
            obj.OnCreateFromPool(this);
            return obj;
        }

        private static void OnTakeFromPool(IPoolClass obj)
        {
        }

        private static void OnReturnToPool(IPoolClass obj)
        {
        }

        private static void OnDestroyItem(IPoolClass obj)
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
    public class ClassObjectPoolMgr
    {
        private readonly Dictionary<Type, ClassObjectPool> _objectPoolMap = new();
        private const int DefaultCapacity = 32;
        private const int DefaultMaxSize = 10000;

        public T Get<T>() where T : class, IPoolClass, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                pool = new ClassObjectPool(typeof(T), DefaultCapacity, DefaultMaxSize);
                _objectPoolMap.Add(type, pool);
            }

            return pool.Get() as T;
        }

        public IPoolClass Get(Type type)
        {
            var t = typeof(IPoolClass);
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

        public void Release(IPoolClass obj)
        {
            var type = obj.GetType();
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return;

            pool.Release(obj);
        }
        
        public List<IPoolClass> GetActiveObjects(Type type)
        {
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return null;
            return pool.GetActiveObjects();
        }

        public bool IsActiveObject(IPoolClass obj)
        {
            var type = obj.GetType();
            return _objectPoolMap.TryGetValue(type, out var pool) && pool.IsActiveObject(obj);
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
}