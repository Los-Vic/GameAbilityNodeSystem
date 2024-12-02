using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Float", "Default/Literal/Float", ENodeCategory.Value, ENodeNumsLimit.None, typeof(FloatNodeRunner))]
    public class FloatNode:NodeSystemNode
    {
        [ExposedProp]
        public float Val;

        [Port(Direction.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public class FloatNodeRunner:NodeSystemNodeRunner
    {
        private FloatNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
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