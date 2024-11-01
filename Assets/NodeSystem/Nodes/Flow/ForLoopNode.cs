using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("ForLoop", "Default/FlowControl/ForLoop", ENodeCategory.FlowControl, ENodeNumsLimit.None, typeof(ForLoopNodeRunner))]
    public class ForLoopNode:NodeSystemNode
    {
        [Port(Direction.Input, typeof(ExecutePort))]
        public string InExecPort;

        [Port(Direction.Input, typeof(int), "Start")]
        public string InStartIndex;
        
        [Port(Direction.Input, typeof(int), "End")]
        public string InEndIndex;

        [Port(Direction.Output, typeof(ExecutePort), "Completed")]
        public string OutCompleteExecPort;
        
        [Port(Direction.Output, typeof(ExecutePort),"ForEach")]
        public string OutForEachExecPort;
    }
}