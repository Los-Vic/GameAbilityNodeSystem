using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Float", "Literal/Float", ENodeCategory.Value, ENodeNumsLimit.None, typeof(FloatNodeRunner))]
    public class FloatNode:NodeSystemNode
    {
        [ExposedProp]
        public float Val;

        [Port(Direction.Output, typeof(float))]
        public string OutPortVal;
    }
}