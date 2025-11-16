using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Gameplay.Common
{
    public interface ITable<TKey, TRecord> where TKey : struct
        where TRecord : struct
    {
        public bool HasKey(TKey key);
        public bool TryGet(TKey key, out TRecord record);
        public TRecord Get(TKey key);
        public void Add(TKey key, ref TRecord record);
        public void Remove(TKey key, out TRecord record);
        public void Update(TKey key, ref TRecord record);
    }
    
    /// <summary>
    /// Custom struct must override Equals & GetHashCode methods
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TRecord"></typeparam>
    [Serializable]
    public class DataTable<TKey, TRecord> where TKey : struct
        where TRecord : struct
    {
        private Dictionary<TKey, TRecord> _contents;
        public IReadOnlyDictionary<TKey, TRecord> Contents => _contents;

        internal void Init(int capacity)
        {
            _contents = new Dictionary<TKey, TRecord>(capacity);
        }

        internal bool HasKey(TKey key)
        {
            return _contents.ContainsKey(key);
        }
        
        internal TRecord Get(TKey key)
        {
            return _contents.GetValueOrDefault(key);
        }

        internal bool TryGet(TKey key, out TRecord value)
        {
            return _contents.TryGetValue(key, out value);
        }
        
        internal void Add(TKey key, ref TRecord value)
        {
            _contents.TryAdd(key, value);
        }
        internal void Update(TKey key, ref TRecord value)
        {
            _contents[key] = value;
        }
        

        internal void Remove(TKey key, out TRecord value)
        {
            _contents.Remove(key, out value);
        }

        internal void Clear()
        {
            _contents.Clear();
        }
    }

    public class Table<TKey, TRecord> :ITable<TKey, TRecord>
        where TKey : struct
        where TRecord : struct
    {
        public DataTable<TKey, TRecord> DataTable { get; private set; }
        
        public Action<TKey, TRecord> OnAddRecord;
        public Action<TKey, TRecord> OnRemoveRecord;
        public Action<TKey, TRecord, TRecord> OnUpdateRecord;

        public void Init(int capacity)
        {
            DataTable ??= new DataTable<TKey, TRecord>();
            DataTable.Init(capacity);
        }

        public void UnInit()
        {
            DataTable?.Clear();
            OnAddRecord = null;
            OnRemoveRecord = null;
            OnUpdateRecord = null;
        }

        public void Add(TKey key, ref TRecord record)
        {
            Assert.IsNotNull(DataTable);
            DataTable.Add(key, ref record);
            OnAddRecord?.Invoke(key, record);
        }
        
        public void Remove(TKey key, out TRecord record)
        {
            Assert.IsNotNull(DataTable);
            DataTable.Remove(key, out record);
            OnRemoveRecord?.Invoke(key, record);
        }

        public void Update(TKey key, ref TRecord record)
        {
            Assert.IsNotNull(DataTable);
            var oldRecord = DataTable.Get(key);
            DataTable.Update(key, ref record);
            OnUpdateRecord?.Invoke(key, oldRecord, record);
        }

        public bool HasKey(TKey key)
        {
            Assert.IsNotNull(DataTable);
            return DataTable.HasKey(key);
        }
        
        public TRecord Get(TKey key)
        {
            Assert.IsNotNull(DataTable);
            return DataTable.Get(key);
        }
        
        public bool TryGet(TKey key, out TRecord record)
        {
            Assert.IsNotNull(DataTable);
            return DataTable.TryGet(key, out record);
        }

    }


    /// <summary>
    /// 建议再做一层封装，对事件进行转发
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TRecord"></typeparam>
//     public class DataTableSetEventDispatcher<TTable, TKey, TRecord> where TTable : DataTable<TKey, TRecord>, new()
//         where TKey : struct
//         where TRecord : struct
//     {
//         public Action<Handler<TTable>> OnCreate;
//         public Action<Handler<TTable>> OnDestroy;
//         public Action<Handler<TTable>, TKey, TRecord> OnAddRecord;
//         public Action<Handler<TTable>, TKey, TRecord> OnRemoveRecord;
//         public Action<Handler<TTable>, TKey, TRecord, TRecord> OnUpdateRecord;
//
//         public void Clear()
//         {
//             OnCreate = null;
//             OnDestroy = null;
//             OnAddRecord = null;
//             OnRemoveRecord = null;
//             OnUpdateRecord = null;
//         }
//     }
//     
//     public class DataTableSetAccess<TTable, TKey, TRecord> where TTable: DataTable<TKey, TRecord>, new()
//         where TKey : struct
//         where TRecord : struct
//     {
//         //Handler
//         private readonly HandlerMgr<TTable> _handlerMgr = new();
//         
//         //Callbacks
//         public readonly DataTableSetEventDispatcher<TTable, TKey, TRecord> EventDispatcher = new();
//         
//         //Object pools
//         private readonly Queue<TTable> _tablePoolQueue = new();
//         
//         private int _tableContentCapacity;
//         
//         /// <summary>
//         /// 当数量超过Capacity后，容器会自动扩容
//         /// </summary>
//         /// <param name="handlerCapacity"></param>
//         /// <param name="tableContentCapacity"></param>
//         public void Init(int handlerCapacity , 
//             int tableContentCapacity)
//         {
//             _tableContentCapacity = tableContentCapacity;
//             _handlerMgr.Init(OnCreateDataTable, OnReleaseDataTable, handlerCapacity);
//         }
//
//         public void UnInit()
//         {
//             _handlerMgr.UnInit();
//             EventDispatcher.Clear();
//         }
//         
//         private TTable OnCreateDataTable()
//         {
//             if (_tablePoolQueue.Count != 0) 
//                 return _tablePoolQueue.Dequeue();
//             
//             var t = new TTable();
//             t.Init(_tableContentCapacity);
//             return t;
//         }
//         
//         private void OnReleaseDataTable(Handler<TTable> h, TTable table)
//         {
//             EventDispatcher.OnDestroy?.Invoke(h);
//             table.Clear();
//             _tablePoolQueue.Enqueue(table);
//         }
//         
//         public Handler<TTable> CreateTable()
//         {
//             var h =  _handlerMgr.Create();
//             EventDispatcher.OnCreate?.Invoke(h);
//             return h;
//         }
//
//         public TTable Get(Handler<TTable> handler)
//         {
//             return _handlerMgr.DeRef(handler, out var table) ? table : null;
//         }
//         
//         public void AddRef(Handler<TTable> handler)
//         {
//             _handlerMgr.AddRefCount(handler);
//         }
//
//         public void RemoveRef(Handler<TTable> handler)
//         {
//             _handlerMgr.RemoveRefCount(handler);
//         }
//
//         public void DestroyTable(Handler<TTable> handler)
//         {
//             _handlerMgr.ForceRelease(handler);
//         }
//
//         public void AddRecord(Handler<TTable> h, TKey key, ref TRecord record)
//         {
//             if (!_handlerMgr.DeRef(h, out var table))
//                 return;
//             table.Add(key, ref record);
//             EventDispatcher.OnAddRecord?.Invoke(h, key, record);
//         }
//
//         public void UpdateRecord(Handler<TTable> h, TKey key, ref TRecord record)
//         {
//             if (!_handlerMgr.DeRef(h, out var table))
//                 return;
//             var oldRecord = table.Get(key);
//             table.Update(key, ref record);
//             EventDispatcher.OnUpdateRecord?.Invoke(h, key, oldRecord, record);
//         }
//
//         public void RemoveRecord(Handler<TTable> h, TKey key, out TRecord record)
//         {
//             record = default;
//             if (!_handlerMgr.DeRef(h, out var table))
//                 return;
//             table.Remove(key, out record);
//             EventDispatcher.OnRemoveRecord?.Invoke(h, key, record);
//         }
//     }
}