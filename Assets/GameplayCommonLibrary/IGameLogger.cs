namespace GameplayCommonLibrary
{
    /// <summary>
    /// 为了保证多线程仍可以打印log
    /// </summary>
    
    public interface IGameLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }

    public interface IGameLogMsgSender
    {
        public IGameLogger Logger { get; set; }
    }
}