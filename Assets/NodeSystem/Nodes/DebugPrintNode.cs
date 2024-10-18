using NodeSystem.Ports;
using UnityEditor.Experimental.GraphView;

namespace NodeSystem.Nodes
{
    [Node("DebugPrint","Debug/DebugPrint")]
    public class DebugPrintNode:NodeSystemNode
    {
        [ExposedProp]
        public string Log;

        [Port(Direction.Input, typeof(FlowPort))]
        public string InPort;
        [Port(Direction.Input, typeof(IntPort),"IntValue")]
        public string InInt;
        [Port(Direction.Output, typeof(FlowPort))]
        public string OutPort;
        
    }
}