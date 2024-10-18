using NodeSystem.Ports;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NodeSystem.Nodes
{
    [Node("Int", "Literal/Int", ENodeCategory.Value)]
    public class IntNode:NodeSystemNode
    {
        [ExposedProp]
        public int Val;

        [Port(Direction.Output, typeof(IntPort))]
        public string OutVal;
    }
}