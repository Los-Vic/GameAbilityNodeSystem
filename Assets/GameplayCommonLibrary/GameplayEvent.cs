using System;
using System.Collections.Generic;
using UnityEngine;

namespace GCL
{
    public class GameplayEvent<T>
    {
        private readonly List<Action<T>> _callbackList = new();
        private readonly Dictionary<Action<T>, int> _callbackPriorityMap;
        private readonly bool _usePriority;
        
        public GameplayEvent(bool usePriority = false)
        {
            _usePriority = usePriority;
            if (usePriority)
            {
                _callbackPriorityMap = new Dictionary<Action<T>, int>();
            }
        }
        
        public void AddListener(Action<T> callback, int priority = 0)
        {
            if (_callbackList.Contains(callback))
                return;
            
            _callbackList.Add(callback);
            
            //使用priority
            if(!_usePriority)
                return;
            _callbackPriorityMap.Add(callback, priority);
            //排序，保证priority大的排在前面
            _callbackList.Sort((x, y) =>
            {
                if (_callbackPriorityMap[x] > _callbackPriorityMap[y])
                    return -1;

                if (_callbackPriorityMap[x] < _callbackPriorityMap[y])
                    return 1;
                return 0;
            });
        }
        
        public void RemoveListener(Action<T> callback)
        {
            _callbackList.Remove(callback);
            _callbackPriorityMap?.Remove(callback);
        }

        public void OnEvent(T t)
        {
            foreach (var cb in _callbackList.ToArray())
            {
                try
                {
                    cb?.Invoke(t);
                }
                catch (Exception e)
                {
                    GameLogger.LogError(e.ToString());
                }
            }
        }

        public void Clear()
        {
            _callbackList.Clear();
            _callbackPriorityMap?.Clear();
        }
    }
    
    public class GameplayEvent<T1, T2>
    {
        private readonly List<Action<T1, T2>> _callbackList = new();
        private readonly Dictionary<Action<T1, T2>, int> _callbackPriorityMap;
        private readonly bool _usePriority;
        
        public GameplayEvent(bool usePriority = false)
        {
            _usePriority = usePriority;
            if (usePriority)
            {
                _callbackPriorityMap = new Dictionary<Action<T1, T2>, int>();
            }
        }
        
        public void AddListener(Action<T1, T2> callback, int priority = 0)
        {
            if (_callbackList.Contains(callback))
                return;
            
            _callbackList.Add(callback);
            
            //使用priority
            if(!_usePriority)
                return;
            _callbackPriorityMap.Add(callback, priority);
            //排序，保证priority大的排在前面
            _callbackList.Sort((x, y) =>
            {
                if (_callbackPriorityMap[x] > _callbackPriorityMap[y])
                    return -1;

                if (_callbackPriorityMap[x] < _callbackPriorityMap[y])
                    return 1;
                return 0;
            });
        }
        
        public void RemoveListener(Action<T1, T2> callback)
        {
            _callbackList.Remove(callback);
            _callbackPriorityMap?.Remove(callback);
        }

        public void OnEvent(T1 t1, T2 t2)
        {
            foreach (var cb in _callbackList.ToArray())
            {
                try
                {
                    cb?.Invoke(t1, t2);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void Clear()
        {
            _callbackList.Clear();
            _callbackPriorityMap?.Clear();
        }
    }
    
    public class GameplayEvent<T1, T2, T3>
    {
        private readonly List<Action<T1, T2, T3>> _callbackList = new();
        private readonly Dictionary<Action<T1, T2, T3>, int> _callbackPriorityMap;
        private readonly bool _usePriority;
        
        public GameplayEvent(bool usePriority = false)
        {
            _usePriority = usePriority;
            if (usePriority)
            {
                _callbackPriorityMap = new Dictionary<Action<T1, T2, T3>, int>();
            }
        }
        
        public void AddListener(Action<T1, T2, T3> callback, int priority = 0)
        {
            if (_callbackList.Contains(callback))
                return;
            
            _callbackList.Add(callback);
            
            //使用priority
            if(!_usePriority)
                return;
            _callbackPriorityMap.Add(callback, priority);
            //排序，保证priority大的排在前面
            _callbackList.Sort((x, y) =>
            {
                if (_callbackPriorityMap[x] > _callbackPriorityMap[y])
                    return -1;

                if (_callbackPriorityMap[x] < _callbackPriorityMap[y])
                    return 1;
                return 0;
            });
        }
        
        public void RemoveListener(Action<T1, T2, T3> callback)
        {
            _callbackList.Remove(callback);
            _callbackPriorityMap?.Remove(callback);
        }

        public void OnEvent(T1 t1, T2 t2, T3 t3)
        {
            foreach (var cb in _callbackList.ToArray())
            {
                try
                {
                    cb?.Invoke(t1, t2, t3);
                }
                catch (Exception e)
                {
                    GameLogger.LogError(e.ToString());
                }
            }
        }

        public void Clear()
        {
            _callbackList.Clear();
            _callbackPriorityMap?.Clear();
        }
    }
}