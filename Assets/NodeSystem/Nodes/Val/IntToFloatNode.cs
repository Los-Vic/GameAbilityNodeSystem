using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("IntToFloat", "Default/Converter/IntToFloat", ENodeCategory.Value, ENodeNumsLimit.None, typeof(IntToFloatNodeRunner) )]
    public class IntToFloatNode:NodeSystemNode
    {
        [Port(Direction.Input, typeof(int))]
        public string InPortVal;

        [Port(Direction.Output, typeof(float))]
        public string OutPortVal;
    }
}