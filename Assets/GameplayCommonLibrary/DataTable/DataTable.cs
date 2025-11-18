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
        
        public readonly Event<TKey, TRecord> OnAddRecord = new();
        public readonly Event<TKey, TRecord> OnRemoveRecord = new();
        public readonly Event<TKey, TRecord, TRecord> OnUpdateRecord = new();

        public void Init(int capacity)
        {
            DataTable ??= new DataTable<TKey, TRecord>();
            DataTable.Init(capacity);
        }

        public void UnInit()
        {
            DataTable?.Clear();
            OnAddRecord.Clear();
            OnRemoveRecord.Clear();
            OnUpdateRecord.Clear();
        }

        public void Add(TKey key, ref TRecord record)
        {
            Assert.IsNotNull(DataTable);
            DataTable.Add(key, ref record);
            OnAddRecord.Broadcast(key, record);
        }
        
        public void Remove(TKey key, out TRecord record)
        {
            Assert.IsNotNull(DataTable);
            DataTable.Remove(key, out record);
            OnRemoveRecord.Broadcast(key, record);
        }

        public void Update(TKey key, ref TRecord record)
        {
            Assert.IsNotNull(DataTable);
            var oldRecord = DataTable.Get(key);
            DataTable.Update(key, ref record);
            OnUpdateRecord.Broadcast(key, oldRecord, record);
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
}