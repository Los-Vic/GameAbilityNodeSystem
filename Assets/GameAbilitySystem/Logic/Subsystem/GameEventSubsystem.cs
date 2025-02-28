using System;
using System.Collections.Generic;
using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class GameEventSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<EGameEventType, GameplayEvent<GameEventArg>> _gameEvents = new();

        public override void UnInit()
        {
            _gameEvents.Clear();
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

        internal void PostGameEvent(ref GameEventInitParam param)
        {
            var eventArg = System.GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Get<GameEventArg>();
            eventArg.Init(ref param);
            
            GameLogger.Log($"Post game event, event type:{param.EventType}, event src:{param.EventSrcUnit.UnitName}");
            var gameEvent = GetGameEvent(param.EventType);
            gameEvent.OnEvent(eventArg);
            
            eventArg.GetRefCountDisposableComponent().MarkForDispose();
        }
        
        internal GameplayEvent<GameEventArg> GetGameEvent(EGameEventType gameEventType)
        {
            if (_gameEvents.TryGetValue(gameEventType, out var gameEvent))
                return gameEvent;

            gameEvent = new GameplayEvent<GameEventArg>(true);
            _gameEvents.Add(gameEventType, gameEvent);
            return gameEvent;
        }
        
        
    }
}