using System;
using UnityEngine;

namespace GameplayCommonLibrary
{
    public interface IGameLoggerHandler
    {
        void Log(int threadId, string message);
        void LogWarning(int threadId, string message);
        void LogError(int threadId, string message);
    }

    public class DefaultGameLogger:IGameLoggerHandler
    {
        public void Log(int threadId, string message)
        {
            Debug.Log($"@{threadId}: {message}");
        }

        public void LogWarning(int threadId, string message)
        {
            Debug.LogWarning($"@{threadId}: {message}");
        }

        public void LogError(int threadId, string message)
        {
            Debug.LogError($"@{threadId}: {message}");
        }
    }
    
    public static class GameLogger
    {
        private static IGameLoggerHandler _handler;

        private static IGameLoggerHandler LoggerHandler => _handler ??= new DefaultGameLogger();

        public static void SetHandler(IGameLoggerHandler handler)
        {
            _handler = handler;
        }

        public static void Log(string message)
        {
            LoggerHandler?.Log(Environment.CurrentManagedThreadId, message);
        }
        public static void LogWarning(string message)
        {
            LoggerHandler?.LogWarning(Environment.CurrentManagedThreadId, message);
        }
        public static void LogError(string message)
        {
            LoggerHandler?.LogError(Environment.CurrentManagedThreadId, message);
        }
    }
}