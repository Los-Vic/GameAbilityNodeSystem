using System;
using System.Collections.Generic;

namespace Gameplay.Common
{
    /// <summary>
    /// 因为Unity还不支持.Net 9,无法使用Delegate.InvocationListEnumerator
    /// 使用Try...Catch...，避免Hook的Callback中断逻辑运行
    /// </summary>
    public class Event
    {
        private readonly List<Action> _callbacks = new ();

        public void Register(Action callback)
        {
            _callbacks.Add(callback);
        }

        public void Unregister(Action callback)
        {
            _callbacks.Remove(callback);
        }

        public void Clear()
        {
            _callbacks.Clear();
        }

        public void Broadcast()
        {
            foreach (var callback in _callbacks)
            {
                try
                {
                    callback();
                }
                catch (Exception e)
                {
                    GameLogger.LogError(e.ToString());
                }
            }
        }
    }
    
    public class Event<T1>
    {
        private readonly List<Action<T1>> _callbacks = new ();

        public void Register(Action<T1> callback)
        {
            _callbacks.Add(callback);
        }

        public void Unregister(Action<T1> callback)
        {
            _callbacks.Remove(callback);
        }

        public void Clear()
        {
            _callbacks.Clear();
        }

        public void Broadcast(T1 v1)
        {
            foreach (var callback in _callbacks)
            {
                try
                {
                    callback(v1);
                }
                catch (Exception e)
                {
                    GameLogger.LogError(e.ToString());
                }
            }
        }
    }
    
    
    public class Event<T1, T2>
    {
        private readonly List<Action<T1, T2>> _callbacks = new ();

        public void Register(Action<T1, T2> callback)
        {
            _callbacks.Add(callback);
        }

        public void Unregister(Action<T1, T2> callback)
        {
            _callbacks.Remove(callback);
        }

        public void Clear()
        {
            _callbacks.Clear();
        }

        public void Broadcast(T1 v1, T2 v2)
        {
            foreach (var callback in _callbacks)
            {
                try
                {
                    callback(v1, v2);
                }
                catch (Exception e)
                {
                    GameLogger.LogError(e.ToString());
                }
            }
        }
    }
    
    public class Event<T1, T2, T3>
    {
        private readonly List<Action<T1, T2, T3>> _callbacks = new ();

        public void Register(Action<T1, T2, T3> callback)
        {
            _callbacks.Add(callback);
        }

        public void Unregister(Action<T1, T2, T3> callback)
        {
            _callbacks.Remove(callback);
        }

        public void Clear()
        {
            _callbacks.Clear();
        }

        public void Broadcast(T1 v1, T2 v2, T3 v3)
        {
            foreach (var callback in _callbacks)
            {
                try
                {
                    callback(v1, v2, v3);
                }
                catch (Exception e)
                {
                    GameLogger.LogError(e.ToString());
                }
            }
        }
    }
    
    public class Event<T1, T2, T3, T4>
    {
        private readonly List<Action<T1, T2, T3, T4>> _callbacks = new ();

        public void Register(Action<T1, T2, T3, T4> callback)
        {
            _callbacks.Add(callback);
        }

        public void Unregister(Action<T1, T2, T3, T4> callback)
        {
            _callbacks.Remove(callback);
        }

        public void Clear()
        {
            _callbacks.Clear();
        }

        public void Broadcast(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            foreach (var callback in _callbacks)
            {
                try
                {
                    callback(v1, v2, v3, v4);
                }
                catch (Exception e)
                {
                    GameLogger.LogError(e.ToString());
                }
            }
        }
    }
}