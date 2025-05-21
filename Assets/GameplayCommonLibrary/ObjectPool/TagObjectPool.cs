using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace GameplayCommonLibrary
{
    public interface ITagPoolObject
    {
        void OnCreateFromPool();
        void OnTakeFromPool();
        void OnReturnToPool();
        void OnDestroy();
    }
    
    public class TagObjectPoolMgr
    {
        private readonly Dictionary<string, ObjectPool<ITagPoolObject>> _pools = new();

        public void RegisterPool(string tag, Func<ITagPoolObject> createFunc, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (_pools.ContainsKey(tag))
                return;

            var pool = new ObjectPool<ITagPoolObject>(createFunc, null, null,
                obj => obj.OnDestroy(), 
                true,
                defaultCapacity, maxSize);
            _pools.Add(tag, pool);
            GameLogger.Log($"[TagObjectPoolMgr] register pool, tag:{tag}");
        }

        public void UnregisterPool(string tag)
        {
            _pools.Remove(tag, out var pool);
            GameLogger.Log($"[TagObjectPoolMgr] unregister pool, tag:{tag}, pool stats: inactive:{pool.CountInactive}, all:{pool.CountAll}");
            pool.Dispose();
        }

        public void Clear()
        {
            foreach (var pair in _pools)
            {
                GameLogger.Log($"[TagObjectPoolMgr] clear pool, tag:{pair.Key}, pool stats: inactive:{pair.Value.CountInactive}, all:{pair.Value.CountAll}");
                pair.Value.Dispose();
            }
            _pools.Clear();
        }

        public void ClearByTag(string tag)
        {
            GameLogger.Log($"[TagObjectPoolMgr] clear pool by tag:{tag}");
            foreach (var key in _pools.Keys.ToArray())
            {
                if (key.Contains(tag))
                {
                    UnregisterPool(key);
                }
            }
        }

        public ITagPoolObject GetObject(string tag)
        {
            if (!_pools.TryGetValue(tag, out var pool))
            {
                GameLogger.Log($"[TagObjectPoolMgr] no pool of tag:{tag} exists");
                return null;
            }

            var obj = pool.Get();
            obj.OnTakeFromPool();
            return obj;
        }

        public void ReleaseObject(string tag, ITagPoolObject obj)
        {
            if (!_pools.TryGetValue(tag, out var pool))
            {
                GameLogger.Log($"[TagObjectPoolMgr] no pool of tag:{tag} exists");
                return;
            }
            pool.Release(obj);
            obj.OnReturnToPool();
        }
    }
}