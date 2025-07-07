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
        public bool IsAssigned => _val != 0;
        
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
    
    public class HandlerRscMgr<T>
    {
        private T[] _rscArray;
        private uint[] _generationArray;
        private uint[] _refCountArray;
        
        private uint[] _compactDataArray;  // compactArray -> dataArray , faster to iterate
        private uint _compactDataCount;
        private readonly Dictionary<uint, uint> _dataArrayToCompactArray = new(); // dataArray -> compactArray

        //当_autoIncreaseCounter == dataArray.Count且_freeSlots.Count == 0时，说明dataArray已满，需要resize
        private uint _autoIncreaseCounter;
        private readonly Queue<uint> _freeSlots = new();

        private readonly Action<T> _onHandlerReleased;
        
        public HandlerRscMgr(int arraySize, Action<T> onHandlerReleased)
        {
            _rscArray = new T[arraySize];
            _generationArray = new uint[arraySize];
            _refCountArray = new uint[arraySize];
            _compactDataArray = new uint[arraySize];
            _onHandlerReleased = onHandlerReleased;
        }

        public void Reset()
        {
            for (var i = 0; i < _rscArray.Length; i++)
            {
                _rscArray[i] = default;
                _generationArray[i] = 0;
                _refCountArray[i] = 0;
                _compactDataArray[i] = 0;
            }
            _dataArrayToCompactArray.Clear();
            _compactDataCount = 0;
            _autoIncreaseCounter = 0;
            _freeSlots.Clear();
        }
        
        public Handler<T> CreateHandler(T rsc)
        {
            if (_freeSlots.Count == 0)
            {
                while (true)
                {
                    if(_autoIncreaseCounter == _rscArray.Length)
                        AutoResize();

                    //有可能已经被另一个Create方法绑定了资源了，则继续自增
                    if (_dataArrayToCompactArray.ContainsKey(_autoIncreaseCounter))
                    {
                        _autoIncreaseCounter += 1;
                        continue;
                    }

                    var slot = _autoIncreaseCounter++;
                    _rscArray[slot] = rsc;
                    _dataArrayToCompactArray.Add(slot, _compactDataCount);
                    _compactDataArray[_compactDataCount++] = slot;
                    _generationArray[slot] = 1;
                    
                    var h =  new Handler<T>(slot, 1);
                    AddRefCount(h);
                    return h;
                }
            }

            var freeSlot = _freeSlots.Dequeue();
            _rscArray[freeSlot] = rsc;
            _dataArrayToCompactArray.Add(freeSlot, _compactDataCount);
            _compactDataArray[_compactDataCount++] = freeSlot;

            var h1 = new Handler<T>(freeSlot, _generationArray[freeSlot]);
            AddRefCount(h1);
            return h1;
        }

        //指定rsc和对应的Handler值
        public bool CreateHandler(T rsc, Handler<T> handler)
        {
            if (_dataArrayToCompactArray.ContainsKey(handler.Index))
                return false;
            
            var index = handler.Index;
            if (_rscArray.Length <= index)
            {
                AutoResize(index);
            }
            
            _rscArray[index] = rsc;
            _generationArray[index] = handler.Generation;
            _dataArrayToCompactArray.Add(index, _compactDataCount);
            _compactDataArray[_compactDataCount++] = index;
            AddRefCount(handler);
            return true;
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

            _dataArrayToCompactArray.Remove(index, out var compactId);
            
            var last = _compactDataArray[_compactDataCount - 1];
            _dataArrayToCompactArray[last] = compactId;
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
        
        private void AutoResize(uint toIncludeIndex = 0)
        {
            var newSize = _rscArray.Length * 2;
            if (toIncludeIndex > 0)
            {
                while (newSize <= toIncludeIndex)
                {
                    newSize *= 2;
                }
            }
            
            var newArray = new T[newSize];
            _rscArray.CopyTo(newArray, 0);
            _rscArray = newArray;
            
            var newMagicNumberArray = new uint[newSize];
            _generationArray.CopyTo(newMagicNumberArray, 0);
            _generationArray = newMagicNumberArray;
            
            var newRefCountArray = new uint[newSize];
            _refCountArray.CopyTo(newRefCountArray, 0);
            _refCountArray = newRefCountArray;
            
            var newCompactDataArray = new uint[newSize];
            _compactDataArray.CopyTo(newCompactDataArray, 0);
            _compactDataArray = newCompactDataArray;
        }
    }
    
}