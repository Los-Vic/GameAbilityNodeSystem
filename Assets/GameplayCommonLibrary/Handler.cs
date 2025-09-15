using System;
using System.Collections.Generic; 

namespace GCL
{
    /// <summary>
    ///  1 - 8 bit: Generation ,9 - 32 bit : index
    /// generate: 1 ~ 255
    /// index: 0 ~ 16,777,216
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Handler<T> : IEquatable<Handler<T>>
    {
        private readonly uint _val;
        public Handler(uint index, uint generation)
        {
            _val = (index << 8) + generation;
        }

        private Handler(uint val)
        {
            _val = val;
        }

        public uint Index => _val >> 8;
        public uint Generation => _val & 0xff;
        public bool IsAssigned => _val != 0; // generation不为0
        
        public static implicit operator uint(Handler<T> handler) => handler._val;
        public static implicit operator Handler<T>(uint val) => new Handler<T>(val);

        public bool Equals(Handler<T> other)
        {
            return _val == other._val;
        }

        public override bool Equals(object obj)
        {
            return obj is Handler<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)_val;
        }

        public override string ToString()
        {
            return _val.ToString();
        }
    }

    /// <summary>
    /// 句柄集合类
    /// 功能：使用句柄来管理类的引用，提供引用计数机制，提供索引数组便于遍历
    /// 句柄值有利于记录
    /// 代价：额外的空间，多一次检索（跳转），是否值得？
    /// </summary>
    /// <typeparam name="T">必须是引用类型，实际对象的内存还是由GC管理的，无法减少CacheMiss</typeparam>
    public class HandlerMgr<T> where T : class, new()
    {
        private T[] _rscArray;
        private uint[] _generationArray;
        private uint[] _refCountArray;
        private uint[] _compactDataArray;  // compactArray -> rscArray , faster to iterate , no stable order
        private uint _compactDataCount;  //compactArray中index小于该值的value才是有效的资源
        private uint[] _rscArrayToCompactArrayLookUp; //needed when release handler, rscArray -> compactArray

        //当_autoIncreaseCounter == rscArray.Count且_freeSlots.Count == 0时，说明rscArray已满，需要resize
        private uint _autoIncreaseCounter;
        private Queue<uint> _freeSlots;

        //当handler创建时，申请资源
        private Func<T> _createItemFunc;
        //当handler释放时的回调，用来处理资源释放
        private Action<T> _onReleaseItem;
        private bool _inited;
        
        public void Init(Func<T> createItemFunc, Action<T> onReleaseItem, int arraySize = 64)
        {
            _inited = true;
            _rscArray = new T[arraySize];
            _generationArray = new uint[arraySize];
            _refCountArray = new uint[arraySize];
            _compactDataArray = new uint[arraySize];
            _rscArrayToCompactArrayLookUp = new uint[arraySize];
            _onReleaseItem = onReleaseItem;
            _createItemFunc = createItemFunc;
            _freeSlots = new Queue<uint>(arraySize / 4);
        }

        public void Clear()
        {
            if (!IsInitialized())
                return;
            
            foreach (var rsc in _rscArray)
            {
                _onReleaseItem?.Invoke(rsc);
            }
            
            Array.Clear(_rscArray, 0, _rscArray.Length);
            Array.Clear(_generationArray, 0, _generationArray.Length);
            Array.Clear(_refCountArray, 0, _refCountArray.Length);
            _freeSlots.Clear();
            
            //没必要清理
            //Array.Clear(_compactDataArray, 0, _compactDataArray.Length);
            //Array.Clear(_rscArrayToCompactArrayLookUp, 0,  _rscArrayToCompactArrayLookUp.Length);
            
            _compactDataCount = 0;
            _autoIncreaseCounter = 0;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Handler<T> CreateHandler()
        {
            if (!IsInitialized())
                return 0;

            var rsc = _createItemFunc();
            if (_freeSlots.Count == 0)
            {
                if(_autoIncreaseCounter == _rscArray.Length)
                    AutoResize();

                var slot = _autoIncreaseCounter++;
                _rscArray[slot] = rsc;
                _rscArrayToCompactArrayLookUp[slot] = _compactDataCount;
                _compactDataArray[_compactDataCount++] = slot;
                _generationArray[slot] = 1;
                var h =  new Handler<T>(slot, 1);
                AddRefCount(h);
                return h;
            }

            var freeSlot = _freeSlots.Dequeue();
            _rscArray[freeSlot] = rsc;
            _rscArrayToCompactArrayLookUp[freeSlot] = _compactDataCount;
            _compactDataArray[_compactDataCount++] = freeSlot;

            var h1 = new Handler<T>(freeSlot, _generationArray[freeSlot]);
            AddRefCount(h1);
            return h1;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="rsc"></param>
        /// <returns></returns>
        public bool DeRef(Handler<T> handler, out T rsc)
        {
            rsc = null;
            
            if (!IsInitialized())
                return false;
            
            var index = handler.Index;

            if (!handler.IsAssigned || index >= _rscArray.Length || _generationArray[index] != handler.Generation)
            {
                return false;
            }

            rsc = _rscArray[index];
            return true;
        }

        public bool IsRscValid(Handler<T> handler)
        {
            if (!IsInitialized())
                return false;
            
            var index = handler.Index;
            return handler.IsAssigned && index < _rscArray.Length && _generationArray[index] == handler.Generation;
        }
        
        public void AddRefCount(Handler<T> handler)
        {
            if (!IsInitialized())
                return;
            if (!IsRscValid(handler))
                return;
            if(handler.Index >= _rscArray.Length)
                return;
            _refCountArray[handler.Index] += 1;
        }

        public void RemoveRefCount(Handler<T> handler)
        {
            if (!IsInitialized() || !IsRscValid(handler))
                return;
            _refCountArray[handler.Index] -= 1;
            
            if (_refCountArray[handler.Index] == 0)
            {
                ReleaseHandler(handler);
            }
        }

        public void ForceRelease(Handler<T> handler)
        {
            if (!IsInitialized() || !IsRscValid(handler))
                return;
            ReleaseHandler(handler);
        }

        //ReSharper restore Unity.ExpensiveCode
        public T[] GetAllRsc()
        {
            if(!IsInitialized() || _compactDataCount == 0)
                return Array.Empty<T>();
            
            var rscArray = new T[_compactDataCount];
            for (var i = 0; i < _compactDataCount; i++)
            {
                rscArray[i] = _rscArray[_compactDataArray[i]];
            }
            return rscArray;
        }

        public void ForeachRsc(Action<T> action)
        {
            if (!IsInitialized())
                return;
            for (var i = 0; i < _compactDataCount; i++)
            {
                action(_rscArray[_compactDataArray[i]]);
            }
        }

        public bool IsInitialized()
        {
            return _inited;
        }
        
        // private uint GetRefCount(Handler<T> handler)
        // {
        //     if (!IsRscValid(handler))
        //         return 0;
        //     if (handler.Index >= _dataArray.Length)
        //         return 0;
        //     return _refCountArray[handler.Index];
        // }
        
        /// <summary>
        /// Not check ref count here
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        private void ReleaseHandler(Handler<T> handler)
        {
            var index = handler.Index;

            if (!handler.IsAssigned && index >= _rscArray.Length)
                return;
            
            if (_generationArray[index] != handler.Generation)
                return;
            
            //remain handler valid when call OnHandlerReleased
            var rsc = _rscArray[index];
            _onReleaseItem(rsc);
            
            var generation = _generationArray[index];
            if (generation == 0xffu)
            {
                generation = 1;
            }
            else
            {
                generation += 1;
            }
            
            _generationArray[index] = generation;
            _freeSlots.Enqueue(index);
            _rscArray[index] = null;
            //_rscArrayToCompactArrayLookUp[index] = 0;
            var compactId = _rscArrayToCompactArrayLookUp[index];
            var last = _compactDataArray[_compactDataCount - 1];
            _rscArrayToCompactArrayLookUp[last] = compactId;
            _compactDataArray[compactId] = last;
            _compactDataCount--;
        }
        
        //ReSharper restore Unity.ExpensiveCode
        private void AutoResize()
        {
            var newSize = _rscArray.Length * 2;
            Array.Resize(ref _rscArray, newSize);
            Array.Resize(ref _generationArray, newSize);
            Array.Resize(ref _refCountArray, newSize);
            Array.Resize(ref _compactDataArray, newSize);
            Array.Resize(ref _rscArrayToCompactArrayLookUp, newSize);
        }
    }
}