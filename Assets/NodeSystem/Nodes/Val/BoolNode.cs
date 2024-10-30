using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Bool", "Literal/Bool", ENodeCategory.Value, ENodeNumsLimit.None, typeof(BoolNodeRunner))]
    public class BoolNode : NodeSystemNode
    {
        [ExposedProp] public bool Val;

        [Port(Direction.Output, typeof(bool))] 
        public string OutPortVal;
    }
}