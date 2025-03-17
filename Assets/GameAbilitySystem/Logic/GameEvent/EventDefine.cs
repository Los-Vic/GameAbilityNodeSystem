using GameplayCommonLibrary;
using MissQ;
using NS;

namespace GAS.Logic
{
    public enum EGameEventType
    {
       None,
       OnPostPrepareStart,
    }

    public struct GameEventInitParam
    {
        public EGameEventType EventType;
        public GameUnit EventSrcUnit; //not null
        public GameAbility EventSrcAbility; //nullable
        public GameEffect EventSrcEffect;  //nullable
        public GameUnit EventTargetUnit;   //nullable
        public FP EventValue1;
        public FP EventValue2;
        public FP EventValue3;
        public string EventString;
    }
    
    public class GameEventArg:IEntryParam, IPoolClass, IRefCountRequester, IRefCountDisposableObj
    {
        public EGameEventType EventType;
        
        public GameUnit EventSrcUnit; //not null
        public GameAbility EventSrcAbility; //nullable
        public GameEffect EventSrcEffect;  //nullable
        public GameUnit EventTargetUnit;   //nullable
        
        public FP EventValue1;
        public FP EventValue2;
        public FP EventValue3;
        public string EventString;

        private bool _isActive;
        private RefCountDisposableComponent _refCountDisposableComponent;
        private ClassObjectPool _pool;

        public void Init(ref GameEventInitParam param)
        {
            EventType = param.EventType;
            EventSrcUnit = param.EventSrcUnit;
            EventSrcAbility = param.EventSrcAbility;
            EventSrcEffect = param.EventSrcEffect;
            EventTargetUnit = param.EventTargetUnit;
            EventValue1 = param.EventValue1;
            EventValue2 = param.EventValue2;
            EventValue3 = param.EventValue3;
            EventString = param.EventString;
        }

        private void Reset()
        {
            EventSrcUnit = null;
            EventSrcAbility = null;
            EventSrcEffect = null;
            EventTargetUnit = null;
        }
        
        #region IPoolObject
        public void OnCreateFromPool(ClassObjectPool pool)
        {
            _pool = pool;
        }

        public void OnTakeFromPool()
        {
            _isActive = true;
        }

        public void OnReturnToPool()
        {
            _isActive = false;
            Reset();
        }
        public void OnDestroy()
        {
        }
        #endregion

        #region IRefCounterRequester

        public bool IsRequesterStillValid()
        {
            return _isActive;
        }

        public string GetRequesterDesc()
        {
            return $"{EventType}";
        }

        #endregion


        #region IRefCountDisposableObj

        public RefCountDisposableComponent GetRefCountDisposableComponent()
        {
            return _refCountDisposableComponent ??= new RefCountDisposableComponent(this);
        }

        public bool IsDisposed()
        {
            return !_isActive;
        }

        public void ForceDisposeObj()
        {
            GetRefCountDisposableComponent().DisposeOwner();
        }

        public void OnObjDispose()
        {
            _pool?.Release(this);
        }

        #endregion
     
    }
}