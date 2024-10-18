using NodeSystem.Ports;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NodeSystem.Nodes
{
    [Node("Start", "Flow/Start", ENodeCategory.Start)]
    public class StartNode:NodeSystemNode
    {
        [Port(Direction.Output, typeof(FlowPort))]
        public string OutPort;
    }
}