using NodeSystem.Ports;
using NodeSystem.Runners;
using UnityEditor.Experimental.GraphView;

namespace NodeSystem.Nodes
{
    [Node("Int", "Literal/Int", ENodeCategory.Value, ENodeNumsLimit.None, typeof(IntNodeRunner))]
    public class IntNode:NodeSystemNode
    {
        [ExposedProp]
        public int Val;

        [Port(Direction.Output, typeof(IntPort))]
        public string OutVal;
    }
}