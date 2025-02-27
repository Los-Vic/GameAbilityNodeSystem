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

        public void RegisterGameEvent(EGameEventType eventType, Action<GameEventArg> callback)
        {
            var gameEvent = GetGameEvent(eventType);
            gameEvent.AddListener(callback);
        }

        public void UnregisterGameEvent(EGameEventType eventType, Action<GameEventArg> callback)
        {
            var gameEvent = GetGameEvent(eventType);
            gameEvent.RemoveListener(callback);
        }

        public void InvokeGameEvent(EGameEventType eventType, GameEventArg arg)
        {
            var gameEvent = GetGameEvent(eventType);
            gameEvent.OnEvent(arg);
        }
        
        private GameplayEvent<GameEventArg> GetGameEvent(EGameEventType gameEventType)
        {
            if (_gameEvents.TryGetValue(gameEventType, out var gameEvent))
                return gameEvent;

            gameEvent = new GameplayEvent<GameEventArg>(true);
            _gameEvents.Add(gameEventType, gameEvent);
            return gameEvent;
        }
        
        
    }
}