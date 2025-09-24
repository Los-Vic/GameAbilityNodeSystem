using System;

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
        public uint Generation => _val & 0xffu;
        public bool IsAssigned => Generation != 0; // generation == 0 表示无效
        
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
    /// 代价：额外的空间，额外的检索，是否值得？
    /// </summary>
    /// <typeparam name="T">必须是引用类型，实际对象的内存还是由GC管理的，无法减少CacheMiss</typeparam>
    public class HandlerMgr<T> where T : class, new()
    {
        //object reference，目前采用和sparseArray对齐
        //这里有分支：和packArray对齐，则遍历不需要额外跳转，检索要跳转两次
        //和sparseArray对齐，则遍历需要一次跳转，检索一次跳转
        private T[] _rscArray; 
        private Handler<T>[] _packedArray; // 用于遍历
        
        //1、用于外部的句柄检索和比对Generation
        //2、隐含一个回收index链表
        private Handler<T>[] _sparseArray; // 用于检索
        
        private ushort[] _refCountArray; // ref count , avoid handler been released， _sparseArray对齐

        private uint _rscNums;  //rsc array valid nums
        private uint _recycledArrayHead; // shifting in _handlers array
        private uint _recycledRscNums; 

        //当handler创建时，申请资源
        private Func<T> _createItemFunc;
        //当handler释放时的回调，用来处理资源释放
        private Action<T> _onReleaseItem;
        
        private bool _inited;

        #region Public Methods

        public void Init(Func<T> createItemFunc, Action<T> onReleaseItem, uint rscCapacity = 64)
        {
            if(_inited)
                return;
            
            _inited = true;
            
            _sparseArray = new Handler<T>[rscCapacity];
            _packedArray = new Handler<T>[rscCapacity];
            _rscArray = new T[rscCapacity];
            _refCountArray = new ushort[rscCapacity];
            
            _onReleaseItem = onReleaseItem;
            _createItemFunc = createItemFunc;
        }

        public void UnInit()
        {
            if (!_inited)
                return;
            
            for (var i = 0; i < _rscNums; i++)
            {
                _onReleaseItem?.Invoke(_rscArray[i]);
            }
            
            _inited = false;
            _createItemFunc = null;
            _onReleaseItem = null;
            
            _packedArray = null;
            _sparseArray = null;
            _rscArray = null;
            _refCountArray = null;
            
            _rscNums = 0;
            _recycledRscNums = 0;
            _recycledArrayHead = 0;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Handler<T> CreateHandler()
        {
            if (!_inited)
                return 0;

            //no capacity
            if (_rscNums == _rscArray.Length)
            {
                AutoResize();
            }
            
            var r = _createItemFunc();
            
            //no recycled slot
            if (_recycledRscNums == 0)
            {
                var h =  new Handler<T>(_rscNums, 1);
                _rscArray[_rscNums] = r;
                _sparseArray[_rscNums] = h;
                _packedArray[_rscNums] = h;
                _refCountArray[_rscNums] += 1;
                _rscNums++;
                return h;
            }
            else
            {
                var slot = _recycledArrayHead;
                var internalHandler = _sparseArray[_recycledArrayHead];
                var generation = internalHandler.Generation;
                _recycledArrayHead = internalHandler.Index;
                _recycledRscNums--;

                var h = new Handler<T>(_rscNums, generation);
                _rscArray[slot] = r;
                _sparseArray[slot] = h;
                _packedArray[_rscNums] = new Handler<T>(slot, generation);
                _refCountArray[slot] += 1;
                _rscNums++;
                return h;
            }
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
            
            if (!IsRscValid(handler))
                return false;
            
            rsc = _rscArray[handler.Index];
            return true;
        }

        public bool IsRscValid(Handler<T> handler)
        {
            if (!_inited)
                return false;

            if (!handler.IsAssigned)
                return false;
            
            var index = handler.Index;
            if (_sparseArray[index].Generation != handler.Generation)
                return false;

            return true;
        }
        
        public void AddRefCount(Handler<T> handler)
        {
            if (!IsRscValid(handler))
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

        public void ForceRelease(Handler<T> handler)
        {
            if (!IsRscValid(handler))
                return;
            ReleaseHandler(handler);
        }
        
        public Handler<T>[] GetAllRscHandlers(out uint rscNums)
        {
            rscNums = _rscNums;
            return rscNums == 0 ? null : _packedArray;
        }

        #endregion

        #region  Private Methods
        
        // private uint GetRefCount(Handler<T> handler)
        // {
        //     if (!IsRscValid(handler))
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
            if (!IsRscValid(handler))
                return;
            
            var index = handler.Index;
            var rsc = _rscArray[index];
            //remain handler valid when call OnHandlerReleased
            _onReleaseItem(rsc);
            
            
            _rscArray[index] = null;
            //Swap
            var slotInPackArray = _sparseArray[index].Index;
            _packedArray[slotInPackArray] = _packedArray[_rscNums - 1];
            _rscNums--;
            
            //New generation
            var generation = handler.Generation;
            if (generation == 0xffu)
            {
                generation = 1;
            }
            else
            {
                generation += 1;
            }
            
            _recycledRscNums++;
            _sparseArray[index] = new Handler<T>(_recycledArrayHead, generation);
            _recycledArrayHead = index;
        }
        
        //ReSharper restore Unity.ExpensiveCode
        private void AutoResize()
        {
            var newSize = _rscArray.Length * 2;
            Array.Resize(ref _rscArray, newSize);
            Array.Resize(ref _packedArray, newSize);
            Array.Resize(ref _sparseArray, newSize);
            Array.Resize(ref _refCountArray, newSize);
        }

        #endregion

    }
}