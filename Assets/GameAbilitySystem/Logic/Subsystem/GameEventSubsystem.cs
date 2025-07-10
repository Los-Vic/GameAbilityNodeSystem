using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;

namespace GAS.Logic
{
    public class GameEventSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<EGameEventType, GameplayEvent<GameEventArg>> _gameEvents = new();
        private readonly Stack<GameEventArg> _eventStack = new();
        private const int StackDepthWarningThreshold = 20;
        private readonly List<GameEventCreateParam> _nextFrameEventsCache = new();
        internal HandlerRscMgr<GameEventArg> GameEventRscMgr { get; private set; }

        public override void Init()
        {
            GameEventRscMgr = new(256, DisposeGameEventArg);
        }

        public override void UnInit()
        {
            _gameEvents.Clear();
            _eventStack.Clear();
            _nextFrameEventsCache.Clear();
        }

        public override void Update(float deltaTime)
        {
            if (_nextFrameEventsCache.Count == 0)
                return;
            
            foreach (var p in _nextFrameEventsCache)
            {
                var param = p;
                RealPostgameEvent(ref param);
            }
            _nextFrameEventsCache.Clear();
        }

        public void RegisterGameEvent(EGameEventType eventType, Action<GameEventArg> callback, int priority = 0)
        {
            var gameEvent = GetGameEvent(eventType);
            gameEvent.AddListener(callback, priority);
        }

        public void UnregisterGameEvent(EGameEventType eventType, Action<GameEventArg> callback)
        {
            var gameEvent = GetGameEvent(eventType);
            gameEvent.RemoveListener(callback);
        }

        internal void PostGameEvent(ref GameEventCreateParam param)
        {
            switch (param.TimePolicy)
            {
                case EGameEventTimePolicy.Immediate:
                    RealPostgameEvent(ref param);
                    break;
                case EGameEventTimePolicy.NextFrame:
                    _nextFrameEventsCache.Add(param);
                    break;
            }
        }

        private void RealPostgameEvent(ref GameEventCreateParam param)
        {
            if(!CheckEventStack(ref param))
                return;
            
            var eventArg = System.ClassObjectPoolSubsystem.Get<GameEventArg>();
            var h = GameEventRscMgr.CreateHandler(eventArg);
            var initParam = new GameEventInitParam()
            {
                CreateParam = param,
                Handler = h
            };
            eventArg.Init(ref initParam);
            
            _eventStack.Push(eventArg);
            GameLogger.Log($"Push game event, event type:{param.EventType}, event src:{param.EventSrcUnit}, stack depth:{_eventStack.Count}");
            
            var gameEvent = GetGameEvent(param.EventType);
            gameEvent.OnEvent(eventArg);
            
            GameLogger.Log($"Pop game event, event type:{param.EventType}, stack depth:{_eventStack.Count}");
            _eventStack.Pop();

            GameEventRscMgr.RemoveRefCount(h);
        }

        private void DisposeGameEventArg(GameEventArg arg)
        {
            System.ClassObjectPoolSubsystem.Release(arg);
        }
        
        internal GameplayEvent<GameEventArg> GetGameEvent(EGameEventType gameEventType)
        {
            if (_gameEvents.TryGetValue(gameEventType, out var gameEvent))
                return gameEvent;

            gameEvent = new GameplayEvent<GameEventArg>(true);
            _gameEvents.Add(gameEventType, gameEvent);
            return gameEvent;
        }

        /// <summary>
        /// 检测event的调用堆栈，避免调用太深，或出现死锁
        /// </summary>
        /// <param name="newParam"></param>
        /// <returns></returns>
        private bool CheckEventStack(ref GameEventCreateParam newParam)
        {
            if (_eventStack.Count >= StackDepthWarningThreshold)
            {
                GameLogger.LogWarning($"Check event stack: depth is too large, cur {_eventStack.Count}");
            }

            foreach (var arg in _eventStack)
            {
                if(!arg.EventSrcUnit.IsAssigned || !newParam.EventSrcUnit.IsAssigned)
                    continue;
                
                if(arg.EventSrcUnit != newParam.EventSrcUnit)
                    continue;

                if (arg.EventSrcAbility.IsAssigned && newParam.EventSrcAbility.IsAssigned &&
                    arg.EventSrcAbility == newParam.EventSrcAbility)
                {
                    GameLogger.LogWarning("Check event stack: ignore new event, same src ability");
                    return false;
                }

                if (arg.EventSrcEffect.IsAssigned && newParam.EventSrcEffect.IsAssigned &&
                    arg.EventSrcEffect == newParam.EventSrcEffect)
                {
                    GameLogger.LogWarning("Check event stack: ignore new event, same src effect");
                    return false;
                }
            }
            
            return true;
        }

        internal static bool CheckEventFilters(GameUnit owner, GameEventArg arg, List<EGameEventFilter> filters)
        {
            if (filters == null || filters.Count == 0)
                return true;

            var srcHandler = arg.EventSrcUnit;
            if (!srcHandler.IsAssigned ||
                !owner.System.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(srcHandler, out var src))
                return false;
            
            foreach (var f in filters)
            {
                switch (f)
                {
                    case EGameEventFilter.SrcIsOwner:
                        if (owner != src)
                            return false;
                        break;
                    case EGameEventFilter.SrcIsNotOwner:
                        if (owner == src)
                            return false;
                        break;
                    case EGameEventFilter.SrcIsSelfUnits:
                        if (owner.GetSimpleAttribute(ESimpleAttributeType.PlayerID) !=
                            src.GetSimpleAttribute(ESimpleAttributeType.PlayerID))
                            return false;
                        break;
                    case EGameEventFilter.SrcIsAllyUnits:
                        if (owner.GetSimpleAttribute(ESimpleAttributeType.PlayerCampID) !=
                            src.GetSimpleAttribute(ESimpleAttributeType.PlayerCampID))
                            return false;
                        if (owner.GetSimpleAttribute(ESimpleAttributeType.PlayerID) ==
                            src.GetSimpleAttribute(ESimpleAttributeType.PlayerID))
                            return false;
                        break;
                    case EGameEventFilter.SrcIsSelfAllyUnits:
                        if (owner.GetSimpleAttribute(ESimpleAttributeType.PlayerCampID) !=
                            src.GetSimpleAttribute(ESimpleAttributeType.PlayerCampID))
                            return false;
                        break;
                    case EGameEventFilter.SrcIsRivalUnits:
                        if (owner.GetSimpleAttribute(ESimpleAttributeType.PlayerCampID) ==
                            src.GetSimpleAttribute(ESimpleAttributeType.PlayerCampID))
                            return false;
                        break;
                }
            }

            return true;
        }
        
    }
}