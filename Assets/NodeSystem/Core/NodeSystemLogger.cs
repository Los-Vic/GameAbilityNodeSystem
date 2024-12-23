using UnityEngine;

namespace NS
{
    public interface ILogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
    
    public static class NodeSystemLogger
    {
        private static ILogger _logger;
        public static void SetLogger(ILogger logger) => _logger = logger;
        
        public static void Log(string message)
        {
            if (_logger != null)
            {
                _logger.Log(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(string message)
        {
            if (_logger != null)
            {
                _logger.LogWarning(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }

        public static void LogError(string message)
        {
            if (_logger != null)
            {
                _logger.LogError(message);
            }
            else
            {
                Debug.LogError(message);
            }
        }
    }
}