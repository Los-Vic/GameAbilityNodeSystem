using System;

namespace GCL.ObjSys
{
    public partial class World : IDisposable
    {
        private readonly int _poolKey;
        private ClassObjectPoolCollection _poolCollection;

        public World(int poolKey)
        {
            _poolKey = poolKey;
        }

        public void Dispose()
        {
            ClassObjectPoolMgr.Release(_poolKey);
        }
    }

    // public partial class World
    // {
    //     public static T Get<T>() where T : class, IPoolClass, new()
    //     {
    //         var obj = _objectPoolCollection.Get<T>();
    //         return obj;
    //     }
    //
    //     public static void Release<T>(T obj) where T : class, IPoolClass, new()
    //     {
    //         _objectPoolCollection.Release(obj);
    //     }
    //     
    //     public static Handler<T> GetHandler<T>() where T : class, IPoolClass, new()
    //     {
    //         if (!HandlerMgrMultiple<T>.IsInitialized())
    //         {
    //             HandlerMgrMultiple<T>.Init(Get<T>, Release);
    //         }
    //
    //         return HandlerMgrMultiple<T>.CreateHandler();
    //     }
    // }
}