using System;

namespace NS
{
    public abstract class NodeSystemEventParamBase
    {
    }

    public enum ENodeEventType
    {
        BeginPlay,
        EndPlay
    }
    
    [Serializable]
    public class NodeEventParam:NodeSystemEventParamBase
    {
        public int IntParam1;
        public int IntParam2;
    }
    
    
}