using System.Collections.Generic;

namespace GameplayCommonLibrary
{
    public interface IRefCountRequester
    {
        bool IsRequesterStillValid();
        string GetRequesterDesc();
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

#if BUILD_DEV || UNITY_EDITOR
        private readonly Dictionary<IRefCountRequester, string> _refCountRequesterMap = new();
#endif

        private readonly List<IRefCountRequester> _refCountRequesters = new();
        private bool _isPendingDispose;

        public RefCountDisposableComponent(IRefCountDisposableObj owner)
        {
            Owner = owner;
        }

        private void Reset()
        {
            _isPendingDispose = false;
            _refCountRequesters.Clear();
#if BUILD_DEV || UNITY_EDITOR
            _refCountRequesterMap.Clear();
#endif
        }

        public void AddRefCount(IRefCountRequester requester)
        {
            if (_refCountRequesters.Contains(requester))
                return;

#if BUILD_DEV || UNITY_EDITOR
            _refCountRequesterMap.TryAdd(requester, requester.GetRequesterDesc());
#endif

            _refCountRequesters.Add(requester);
        }

        public void RemoveRefCount(IRefCountRequester requester)
        {
            if (!_refCountRequesters.Contains(requester))
                return;
#if BUILD_DEV || UNITY_EDITOR
            _refCountRequesterMap.Remove(requester);
#endif
            _refCountRequesters.Remove(requester);
            TryDisposeOwner();
        }

        /// <summary>
        /// Owner的管理者在期望销毁Owner的时候调用
        /// </summary>
        public void MarkForDispose()
        {
            _isPendingDispose = true;
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
            if (_isPendingDispose && RefCount == 0)
            {
                DisposeOwner();
            }
        }

#if BUILD_DEV || UNITY_EDITOR
        public List<string> GetRefLog()
        {
            var log = new List<string>();

            log.Add($"-------Ref log[{RefCount}]-------");
            var count = 0;
            foreach (var pair in _refCountRequesterMap)
            {
                count++;
                log.Add($"[{count}]{pair.Value}");
            }

            log.Add("-------End ref log-------");

            return log;
        }
#endif
    }
}