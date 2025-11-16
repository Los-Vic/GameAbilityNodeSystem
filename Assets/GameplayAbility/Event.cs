using System;
using Gameplay.Common;

namespace Gameplay.Ability
{
    public class Event
    {
        internal event Action InternalEvent;

        internal void Broadcast()
        {
            try
            {
                InternalEvent?.Invoke();
            }
            catch (Exception e)
            {
                GameLogger.LogError(e.ToString());
            }
        }
    }

    public class Event<T>
    {
        internal event Action<T> InternalEvent;

        internal void Broadcast(T value)
        {
            try
            {
                InternalEvent?.Invoke(value);
            }
            catch (Exception e)
            {
                GameLogger.LogError(e.ToString());
            }
        }
    }

    public class Event<T1, T2>
    {
        internal event Action<T1, T2> InternalEvent;
        internal void Broadcast(T1 val1, T2 val2)
        {
            try
            {
                InternalEvent?.Invoke(val1, val2);
            }
            catch (Exception e)
            {
                GameLogger.LogError(e.ToString());
            }
        }
    }

    public class Event<T1, T2, T3>
    {
        internal event Action<T1, T2, T3> InternalEvent;

        internal void Broadcast(T1 val1, T2 val2, T3 val3)
        {
            try
            {
                InternalEvent?.Invoke(val1, val2, val3);
            }
            catch (Exception e)
            {
                GameLogger.LogError(e.ToString());
            }
        }
    }
}