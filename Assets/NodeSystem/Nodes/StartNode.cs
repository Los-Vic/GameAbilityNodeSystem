using NodeSystem.Ports;
using UnityEditor.Experimental.GraphView;

namespace NodeSystem.Nodes
{
    [Node("Start", "Flow/Start", ENodeCategory.Start, ENodeNumsLimit.Singleton)]
    public class StartNode:NodeSystemNode
    {
        [Port(Direction.Output, typeof(FlowPort))]
        public string OutPort;
    }
}