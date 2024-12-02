using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Int", "Default/Literal/Int", ENodeCategory.Value, ENodeNumsLimit.None, typeof(IntNodeRunner))]
    public class IntNode:NodeSystemNode
    {
        [ExposedProp]
        public int Val;

        [Port(Direction.Output, typeof(int))]
        public string OutPortVal;
    }
    
    public class IntNodeRunner:NodeSystemNodeRunner
    {
        private IntNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
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