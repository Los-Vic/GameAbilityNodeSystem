using NodeSystem.Core;
using NodeSystem.Runners;
using UnityEditor.Experimental.GraphView;

namespace NodeSystem.Nodes
{
    [Node("Float", "Literal/Float", ENodeCategory.Value, ENodeNumsLimit.None, typeof(FloatNodeRunner))]
    public class FloatNode:NodeSystemNode
    {
        [ExposedProp]
        public float Val;

        [Port(Direction.Output, typeof(float))]
        public string OutVal;
    }
}