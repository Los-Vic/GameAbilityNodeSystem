using NS.Nodes;
using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Float", "Default/Literal/Float", (int)ENodeCategory.Value, ENodeFunctionType.Value, typeof(FloatNodeRunner))]
    public class FloatNode:Node
    {
        [ExposedProp]
        public float Val;

        [Port(Direction.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public class FloatNodeRunner:NodeRunner
    {
        private FloatNode _node;
        private NodeGraphRunner _graphRunner;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = ((FloatNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute()
        {
            _graphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}