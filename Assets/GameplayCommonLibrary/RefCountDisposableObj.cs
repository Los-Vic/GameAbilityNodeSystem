using System.Collections.Generic;

namespace GameplayCommonLibrary
{
    public interface IRefCountRequester
    {
        bool IsRequesterStillValid();
    }

    public interface IRefCountDisposableObj
    {
        RefCountDisposableComponent GetRefCountDisposableComponent();
        bool IsDisposed();
        void ForceDisposeObj();
        void OnObjDispose();
    }

    /// <summary>
    /// 主要作用是：当期望销毁Owner时，有可能存在一些对象仍需要Owner, 这些对象通过Add/RemoveRefCount增加引用来推迟Owner的销毁
    /// </summary>
    public class RefCountDisposableComponent
    {
        public IRefCountDisposableObj Owner { get; }

        public int RefCount
        {
            get
            {
                var count = 0;
                for (var i = _refCountRequesters.Count - 1; i >= 0; --i)
                {
                    var requester = _refCountRequesters[i];
                    //如果requester被销毁或是无效了，就不再认为是一个有效引用
                    if (requester != null && requester.IsRequesterStillValid())
                    {
                        count++;
                    }
                    else
                    {
                        _refCountRequesters.RemoveAt(i);
                    }
                }

                return count;
            }
        }

        private readonly List<IRefCountRequester> _refCountRequesters = new();
        public bool IsPendingDispose { get; private set; }

        public RefCountDisposableComponent(IRefCountDisposableObj owner)
        {
            Owner = owner;
        }

        private void Reset()
        {
            IsPendingDispose = false;
            _refCountRequesters.Clear();
        }

        public void AddRefCount(IRefCountRequester requester)
        {
            if (_refCountRequesters.Contains(requester))
                return;
            _refCountRequesters.Add(requester);
        }

        public void RemoveRefCount(IRefCountRequester requester)
        {
            if (!_refCountRequesters.Contains(requester))
                return;
            _refCountRequesters.Remove(requester);
            TryDisposeOwner();
        }

        /// <summary>
        /// Owner的管理者在期望销毁Owner的时候调用
        /// </summary>
        public void MarkForDispose()
        {
            IsPendingDispose = true;
            TryDisposeOwner();
        }

        public void DisposeOwner()
        {
            Reset();
            if(Owner.IsDisposed())
                return;
            
            Owner.OnObjDispose();
        }

        private void TryDisposeOwner()
        {
            if (IsPendingDispose && RefCount == 0)
            {
                DisposeOwner();
            }
        }
    }
}