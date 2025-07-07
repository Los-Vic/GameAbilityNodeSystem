using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;
using MissQ;
using NS;

namespace GAS.Logic
{
    public class GameEventArg:IEntryParam, IPoolClass
    {
        public EGameEventType EventType;
        
        public Handler<GameUnit> EventSrcUnit; 
        public Handler<GameAbility> EventSrcAbility; 
        public Handler<GameEffect> EventSrcEffect;  
        public Handler<GameUnit> EventTargetUnit;  
        
        public FP EventValue1;
        public FP EventValue2;
        public FP EventValue3;
        public string EventString;

        public Handler<GameEventArg> Handler;

        public void Init(ref GameEventInitParam param)
        {
            EventType = param.CreateParam.EventType;
            EventSrcUnit = param.CreateParam.EventSrcUnit;
            EventSrcAbility = param.CreateParam.EventSrcAbility;
            EventSrcEffect = param.CreateParam.EventSrcEffect;
            EventTargetUnit = param.CreateParam.EventTargetUnit;
            EventValue1 = param.CreateParam.EventValue1;
            EventValue2 = param.CreateParam.EventValue2;
            EventValue3 = param.CreateParam.EventValue3;
            EventString = param.CreateParam.EventString;
            Handler = param.Handler;
        }
        
        #region IPoolObject
        public void OnCreateFromPool()
        {
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
        }
        public void OnDestroy()
        {
        }
        #endregion
    }
}