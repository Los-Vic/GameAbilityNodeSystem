using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Int", "Literal/Int", ENodeCategory.Value, ENodeNumsLimit.None, typeof(IntNodeRunner))]
    public class IntNode:NodeSystemNode
    {
        [ExposedProp]
        public int Val;

        [Port(Direction.Output, typeof(int))]
        public string OutPortVal;
    }
}