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
    
    public enum EReleaseHandlerResult
    {
        Success,
        GenerationMismatch,
        RefCountNotZero,
        InvalidHandler,
    }
    
    public class HandlerResourceMgr<T>
    {
        private T[] _dataArray;
        private uint[] _generationArray;
        private uint[] _refCountArray;
        
        private uint[] _compactDataArray;  // compactArray -> dataArray , faster to iterate
        private uint _compactDataCount;
        private readonly Dictionary<uint, uint> _dataArrayToCompactArray = new(); // dataArray -> compactArray

        //当_autoIncreaseCounter == dataArray.Count且_freeSlots.Count == 0时，说明dataArray已满，需要resize
        private uint _autoIncreaseCounter;
        private readonly Queue<uint> _freeSlots = new();
        
        public HandlerResourceMgr(int arraySize)
        {
            _dataArray = new T[arraySize];
            _generationArray = new uint[arraySize];
            _refCountArray = new uint[arraySize];
            _compactDataArray = new uint[arraySize];
        }

        public Handler<T> Create(T rsc)
        {
            if (_freeSlots.Count == 0)
            {
                while (true)
                {
                    if(_autoIncreaseCounter == _dataArray.Length)
                        AutoResize();

                    //有可能已经被另一个Create方法绑定了资源了，则继续自增
                    if (_dataArrayToCompactArray.ContainsKey(_autoIncreaseCounter))
                    {
                        _autoIncreaseCounter += 1;
                        continue;
                    }

                    var slot = _autoIncreaseCounter++;
                    _dataArray[slot] = rsc;
                    _dataArrayToCompactArray.Add(slot, _compactDataCount);
                    _compactDataArray[_compactDataCount++] = slot;
                    _generationArray[slot] = 1;
                    
                    return new Handler<T>(slot, 1);
                }
            }

            var freeSlot = _freeSlots.Dequeue();
            _dataArray[freeSlot] = rsc;
            _dataArrayToCompactArray.Add(freeSlot, _compactDataCount);
            _compactDataArray[_compactDataCount++] = freeSlot;
            
            return new Handler<T>(freeSlot, _generationArray[freeSlot]);
        }

        //指定rsc和对应的Handler值
        public bool Create(T rsc, Handler<T> handler)
        {
            if (_dataArrayToCompactArray.ContainsKey(handler.Index))
                return false;
            
            var index = handler.Index;
            if (_dataArray.Length <= index)
            {
                AutoResize(index);
            }
            
            _dataArray[index] = rsc;
            _generationArray[index] = handler.Generation;
            _dataArrayToCompactArray.Add(index, _compactDataCount);
            _compactDataArray[_compactDataCount++] = index;
            return true;
        }
        
        public bool Dereference(Handler<T> handler, out T rsc)
        {
            var index = handler.Index;

            if (!handler.IsAssigned || index >= _dataArray.Length || _generationArray[index] != handler.Generation)
            {
                rsc = default;
                return false;
            }

            rsc = _dataArray[index];
            return true;
        }

        public bool IsRscValid(Handler<T> handler)
        {
            var index = handler.Index;
            return handler.IsAssigned && index < _dataArray.Length && _generationArray[index] == handler.Generation;
        }

        //如果需要阻止Handler被Release，则可以给Handler增加引用计数。但切记要调用RemoveRefCount，否则Handler无法被Release
        public void AddRefCount(Handler<T> handler)
        {
            if (!IsRscValid(handler))
                return;
            if(handler.Index >= _dataArray.Length)
                return;
            _refCountArray[handler.Index] += 1;
        }

        public void RemoveRefCount(Handler<T> handler)
        {
            if(handler.Index >= _dataArray.Length)
                return;
            if (_refCountArray[handler.Index] != 0)
            {
                _refCountArray[handler.Index] -= 1;
            }
        }

        public uint GetRefCount(Handler<T> handler)
        {
            if (handler.Index >= _dataArray.Length)
                return 0;
            return _refCountArray[handler.Index];
        }
        
        public EReleaseHandlerResult Release(Handler<T> handler)
        {
            var index = handler.Index;

            if (!handler.IsAssigned && index >= _dataArray.Length)
                return EReleaseHandlerResult.InvalidHandler;
            
            if (_generationArray[index] != handler.Generation)
                return EReleaseHandlerResult.GenerationMismatch;

            if (_refCountArray[index] != 0)
                return EReleaseHandlerResult.RefCountNotZero;

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
            _dataArray[index] = default;

            _dataArrayToCompactArray.Remove(index, out var compactId);
            
            var last = _compactDataArray[_compactDataCount - 1];
            _dataArrayToCompactArray[last] = compactId;
            _compactDataArray[compactId] = last;
            _compactDataCount--;
            
            return EReleaseHandlerResult.Success;
        }

        public T[] GetAllResources()
        {
            if(_compactDataCount == 0)
                return Array.Empty<T>();
            
            var rscArray = new T[_compactDataCount];
            for (var i = 0; i < _compactDataCount; i++)
            {
                rscArray[i] = _dataArray[_compactDataArray[i]];
            }
            return rscArray;
        }

        public void ForeachResource(Action<T> action)
        {
            for (var i = 0; i < _compactDataCount; i++)
            {
                action(_dataArray[_compactDataArray[i]]);
            }
        }
        
        private void AutoResize(uint toIncludeIndex = 0)
        {
            var newSize = _dataArray.Length * 2;
            if (toIncludeIndex > 0)
            {
                while (newSize <= toIncludeIndex)
                {
                    newSize *= 2;
                }
            }
            
            var newArray = new T[newSize];
            _dataArray.CopyTo(newArray, 0);
            _dataArray = newArray;
            
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