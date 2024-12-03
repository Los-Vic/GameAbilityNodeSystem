using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Int", "Default/Literal/Int", (int)ENodeCategory.Value, typeof(IntNodeRunner))]
    public class IntNode:Node
    {
        [ExposedProp]
        public int Val;

        [Port(Direction.Output, typeof(int))]
        public string OutPortVal;
    }
    
    public class IntNodeRunner:NodeRunner
    {
        private IntNode _node;
        private NodeGraphRunner _graphRunner;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = ((IntNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute()
        {
            _graphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}