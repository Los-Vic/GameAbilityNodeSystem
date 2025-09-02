using System;
using System.Collections.Generic; 

namespace GameplayCommonLibrary.Handler
{
    /// <summary>
    ///  1 - 8 bit: Generation ,9 - 32 bit : index
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
    /// 句柄资源管理类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HandlerRscMgr<T>
    {
        private T[] _rscArray;
        private uint[] _generationArray;
        private uint[] _refCountArray;
        
        private uint[] _compactDataArray;  // compactArray -> rscArray , faster to iterate , no stable order
        private uint _compactDataCount;  //compactArray中index小于该值的value才是有效的资源

        private uint[] _rscArrayToCompactArrayLookUp; //needed when release handler, rscArray -> compactArray

        //当_autoIncreaseCounter == rscArray.Count且_freeSlots.Count == 0时，说明rscArray已满，需要resize
        private uint _autoIncreaseCounter;
        private readonly Queue<uint> _freeSlots;

        //当handler释放时的回调，用来处理资源释放
        private readonly Action<T> _onHandlerReleased;
        
        public HandlerRscMgr(int arraySize = 64, Action<T> onHandlerReleased = null)
        {
            _rscArray = new T[arraySize];
            _generationArray = new uint[arraySize];
            _refCountArray = new uint[arraySize];
            _compactDataArray = new uint[arraySize];
            _rscArrayToCompactArrayLookUp = new uint[arraySize];
            _onHandlerReleased = onHandlerReleased;
            _freeSlots = new Queue<uint>(arraySize / 4);
        }

        public void Clear()
        {
            foreach (var rsc in _rscArray)
            {
                _onHandlerReleased?.Invoke(rsc);
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
        
        public Handler<T> CreateHandler(T rsc)
        {
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
        
        public bool Dereference(Handler<T> handler, out T rsc)
        {
            var index = handler.Index;

            if (!handler.IsAssigned || index >= _rscArray.Length || _generationArray[index] != handler.Generation)
            {
                rsc = default;
                return false;
            }

            rsc = _rscArray[index];
            return true;
        }

        public bool IsRscValid(Handler<T> handler)
        {
            var index = handler.Index;
            return handler.IsAssigned && index < _rscArray.Length && _generationArray[index] == handler.Generation;
        }
        
        public void AddRefCount(Handler<T> handler)
        {
            if (!IsRscValid(handler))
                return;
            if(handler.Index >= _rscArray.Length)
                return;
            _refCountArray[handler.Index] += 1;
        }

        public void RemoveRefCount(Handler<T> handler)
        {
            if (!IsRscValid(handler))
                return;
            _refCountArray[handler.Index] -= 1;
            
            if (_refCountArray[handler.Index] == 0)
            {
                ReleaseHandler(handler);
            }
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
            _onHandlerReleased?.Invoke(rsc);
            
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
            _rscArray[index] = default;
            //_rscArrayToCompactArrayLookUp[index] = 0;
            var compactId = _rscArrayToCompactArrayLookUp[index];
            var last = _compactDataArray[_compactDataCount - 1];
            _rscArrayToCompactArrayLookUp[last] = compactId;
            _compactDataArray[compactId] = last;
            _compactDataCount--;
        }

        public T[] GetAllResources()
        {
            if(_compactDataCount == 0)
                return Array.Empty<T>();
            
            var rscArray = new T[_compactDataCount];
            for (var i = 0; i < _compactDataCount; i++)
            {
                rscArray[i] = _rscArray[_compactDataArray[i]];
            }
            return rscArray;
        }

        public void ForeachResource(Action<T> action)
        {
            for (var i = 0; i < _compactDataCount; i++)
            {
                action(_rscArray[_compactDataArray[i]]);
            }
        }
        
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