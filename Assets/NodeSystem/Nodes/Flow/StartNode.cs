using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Start", "Flow/Start", ENodeCategory.Start, ENodeNumsLimit.Singleton, typeof(StartNodeRunner))]
    public class StartNode:NodeSystemNode
    {
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;
    }
}