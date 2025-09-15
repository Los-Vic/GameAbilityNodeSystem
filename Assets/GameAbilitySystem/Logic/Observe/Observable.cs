using System;
using System.Collections.Generic;
using GCL;

namespace GAS.Logic
{
    /// <summary>
    /// 适合匿名函数的callback注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Observable<T>
    {
        private readonly List<object> _observers = new();
        private readonly Dictionary<object, int> _priorityMap = new();
        private readonly Dictionary<object, Action<T>> _callbackMap = new();
        
        public void RegisterObserver(object observer, Action<T> callback, int priority = 0)
        {
            if(_observers.Contains(observer))
                return;
            
            _observers.Add(observer);
            _priorityMap.Add(observer, priority);
            _callbackMap.Add(observer, callback);
            
            _observers.Sort((x, y) =>
            {
                if (_priorityMap[x] > _priorityMap[y])
                    return -1;
                if (_priorityMap[x] < _priorityMap[y])
                    return 1;
                return 0;
            });
        }
        
        public void UnRegisterObserver(object observer)
        {
            _observers.Remove(observer);
            _priorityMap.Remove(observer);
            _callbackMap.Remove(observer);
        }
        
        public void NotifyObservers(T msg)
        {
            foreach (var observer in _observers)
            {
                _callbackMap[observer]?.SafeInvoke(msg);
            }
        }

        public void Clear()
        {
            _observers.Clear();
            _callbackMap.Clear();
            _priorityMap.Clear();
        }
    }
}