﻿using System;
using System.Collections.Generic;
using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class GameEventSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<EGameEventType, GameplayEvent<GameEventArg>> _gameEvents = new();
        private readonly Stack<GameEventArg> _eventStack = new();
        private const int StackDepthWarningThreshold = 20;

        public override void UnInit()
        {
            _gameEvents.Clear();
            _eventStack.Clear();
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
            if(!CheckEventStack(ref param))
                return;
            
            var eventArg = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<GameEventArg>();
            eventArg.Init(ref param);
            
            _eventStack.Push(eventArg);
            GameLogger.Log($"Push game event, event type:{param.EventType}, event src:{param.EventSrcUnit.UnitName}, stack depth:{_eventStack.Count}");
            
            var gameEvent = GetGameEvent(param.EventType);
            gameEvent.OnEvent(eventArg);
            
            GameLogger.Log($"Pop game event, event type:{param.EventType}, stack depth:{_eventStack.Count}");
            _eventStack.Pop();
            
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

        /// <summary>
        /// 检测event的调用堆栈，避免调用太深，或出现死锁
        /// </summary>
        /// <param name="newParam"></param>
        /// <returns></returns>
        private bool CheckEventStack(ref GameEventInitParam newParam)
        {
            if (_eventStack.Count >= StackDepthWarningThreshold)
            {
                GameLogger.LogWarning($"Check event stack: depth is too large, cur {_eventStack.Count}");
            }

            foreach (var arg in _eventStack)
            {
                if(arg.EventSrcUnit != newParam.EventSrcUnit)
                    continue;

                if (arg.EventSrcAbility != null && newParam.EventSrcAbility != null)
                {
                    if (arg.EventSrcAbility == newParam.EventSrcAbility)
                    {
                        GameLogger.LogWarning("Check event stack: ignore new event, same src ability");
                        return false;
                    }
                }
                
                if (arg.EventSrcEffect != null && newParam.EventSrcEffect != null)
                {
                    if (arg.EventSrcEffect == newParam.EventSrcEffect)
                    {
                        GameLogger.LogWarning("Check event stack: ignore new event, same src effect");
                        return false;
                    }
                }
            }
            
            return true;
        }
        
    }
}