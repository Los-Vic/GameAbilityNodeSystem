using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NodeSystem.Nodes
{
    [Node("DebugPrint","Debug/DebugPrint")]
    public class DebugPrintNode:NodeSystemNode
    {
        [ExposedProp]
        public string Log;

        [Port(Direction.Input), SerializeReference]
        public NodeSystemNode InPort;
        [Port(Direction.Output), SerializeReference]
        public NodeSystemNode OutPort;
        
    }
}