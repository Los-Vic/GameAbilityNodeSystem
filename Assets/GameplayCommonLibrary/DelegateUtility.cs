using System;
using UnityEngine;

namespace GameplayCommonLibrary
{
    public static class DelegateUtility
    {
        #region Action

        public static void SafeInvoke(this Action action, Action onException = null)
        {
            foreach (var d in action.GetInvocationList())
            {
                if (d is not Action a)
                    continue;
                try
                {
                    a.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke();
                }
            }
        }

        public static void SafeInvoke<T>(this Action<T> action, T t, Action<T> onException = null)
        {
            foreach (var d in action.GetInvocationList())
            {
                if (d is not Action<T> a)
                    continue;
                try
                {
                    a.Invoke(t);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke(t);
                }
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 t1, T2 t2,
            Action<T1, T2> onException = null)
        {
            foreach (var d in action.GetInvocationList())
            {
                if (d is not Action<T1, T2> a)
                    continue;
                try
                {
                    a.Invoke(t1, t2);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke(t1, t2);
                }
            }
        }


        public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3,
            Action<T1, T2, T3> onException = null)
        {
            foreach (var d in action.GetInvocationList())
            {
                if (d is not Action<T1, T2, T3> a)
                    continue;
                try
                {
                    a.Invoke(t1, t2, t3);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke(t1, t2, t3);
                }
            }
        }

        public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3,
            T4 t4, Action<T1, T2, T3, T4> onException = null)
        {
            foreach (var d in action.GetInvocationList())
            {
                if (d is not Action<T1, T2, T3, T4> a)
                    continue;
                try
                {
                    a.Invoke(t1, t2, t3, t4);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke(t1, t2, t3, t4);
                }
            }
        }

        public static void SafeInvoke<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2,
            T3 t3, T4 t4, T5 t5, Action<T1, T2, T3, T4, T5> onException = null)
        {
            foreach (var d in action.GetInvocationList())
            {
                if (d is not Action<T1, T2, T3, T4, T5> a)
                    continue;
                try
                {
                    a.Invoke(t1, t2, t3, t4, t5);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke(t1, t2, t3, t4, t5);
                }
            }
        }

        #endregion


        //需要注意Func的调用与C#默认不太一样，如果使用了+=，默认是会执行所有回调，并返回最后一个回调的结果值。考虑到这不是一个常见的应用，这里只会执行第一个回调并返回。

        #region Function

        public static TResult SafeInvoke<TResult>(this Func<TResult> func, Action onException = null)
        {
            foreach (var d in func.GetInvocationList())
            {
                if (d is not Func<TResult> a)
                    continue;
                try
                {
                    return a.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke();
                }
            }

            return default;
        }

        public static TResult SafeInvoke<T, TResult>(this Func<T, TResult> func, T t, Action<T> onException = null)
        {
            foreach (var d in func.GetInvocationList())
            {
                if (d is not Func<T, TResult> a)
                    continue;
                try
                {
                    return a.Invoke(t);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke(t);
                }
            }

            return default;
        }

        public static TResult SafeInvoke<T1, T2, TResult>(this Func<T1, TResult> func, T1 t1, T2 t2,
            Action<T1, T2> onException = null)
        {
            foreach (var d in func.GetInvocationList())
            {
                if (d is not Func<T1, T2, TResult> a)
                    continue;
                try
                {
                    return a.Invoke(t1, t2);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    onException?.Invoke(t1, t2);
                }
            }

            return default;
        }

        #endregion
    }
}