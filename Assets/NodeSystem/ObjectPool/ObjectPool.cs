using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeSystem.ObjectPool
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
    

    public class ObjectPool
    {
        private readonly UnityEngine.Pool.ObjectPool<IPoolObject> _pool;
        private readonly List<IPoolObject> _activeObjects = new List<IPoolObject>();
        private readonly Type _poolObjectType;
        
        //Constructor
        public ObjectPool(ObjectPoolParam param, Type type)
        {
            var t = typeof(IPoolObject);
            if (!t.IsAssignableFrom(type))
            {
                Debug.LogError($"[ObjectPool]Create object pool failed: type [{type}] can not assign to IPoolObject");
                return;
            }

            Debug.Log($"[ObjectPool]Create object pool success: type [{type}]");
            _poolObjectType = type;
            _pool = new UnityEngine.Pool.ObjectPool<IPoolObject>(CreateItem, OnTakeFromPool, OnReturnToPool, OnDestroyItem,
                true, param.Capacity, param.MaxSize);
        }
        
        public IPoolObject CreateObject()
        {
            var obj = _pool.Get();
            _activeObjects.Add(obj);
            obj.OnTakeFromPool();
            return obj;
        }

        public void DestroyObject(IPoolObject obj)
        {
            if(!_activeObjects.Remove(obj))
                return;
            
            obj.OnReturnToPool();
            _pool.Release(obj);
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
            //this.LogWarning($"[EF]ObjectPool[{typeof(T).Name}] reach max size! consider increasing max size or decreasing the number of instances!");
            obj.OnDestroy();
        }
        #endregion


        #region Interface

        public (int, int) LogState()
        {
           Debug.Log($"[ObjectPool][{_poolObjectType.Name}]: Active/Total/PoolActive:[{_activeObjects.Count}/{_pool.CountAll}/{_pool.CountActive}]");
           if (_activeObjects.Count != _pool.CountActive)
           {
               Debug.LogError($"[ObjectPool][{_poolObjectType.Name}] has different active count: Active/Total/PoolActive:[{_activeObjects.Count}/{_pool.CountAll}/{_pool.CountActive}]");
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
        private readonly Dictionary<Type, ObjectPool> _objectPoolMap = new();
        public T CreateObject<T>() where T:class, IPoolObject, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                var defaultObj = new T();
                pool = new ObjectPool(defaultObj.GetPoolParam(), typeof(T));
                _objectPoolMap.Add(type, pool);
            }
            
            return pool.CreateObject() as T;
        }

        public IPoolObject CreateObject(Type type)
        {
            var t = typeof(IPoolObject);
            if (!t.IsAssignableFrom(type))
            {
                Debug.LogError($"Create object failed: type {type} can not assign to IPoolObject");
                return default;
            }
            
            if (!_objectPoolMap.TryGetValue(type, out var pool))
            {
                var defaultObj = Activator.CreateInstance(type) as IPoolObject;
                pool = new ObjectPool(defaultObj.GetPoolParam(), type);
                _objectPoolMap.Add(type, pool);
            }
            
            return pool.CreateObject();
        }
        
        public void DestroyObject<T>(T obj) where T:class, IPoolObject, new()
        {
            var type = typeof(T);
            if (!_objectPoolMap.TryGetValue(type, out var pool))
                return;
            
            pool.DestroyObject(obj);
        }

        public IEnumerable<ObjectPool> GetAllPools()
        {
            return _objectPoolMap.Values;
        }
    }
}