using MissQ;

namespace NS
{
    [Node("FP", "Common/LiteralValue/FP", ENodeFunctionType.Value , typeof(FPNodeRunner), CommonNodeCategory.Value)]
    public class FPNode:Node
    {
        [ExposedProp]
        public float Val;

        [Port(EPortDirection.Output, typeof(FP))]
        public string OutPortVal;
    }
    
    public class FPNodeRunner:NodeRunner
    {
        private FPNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = ((FPNode)nodeAsset);
        }

        public override void Execute()
        {
            GraphRunner.SetOutPortVal(_node.OutPortVal, (FP)_node.Val);
        }
    }
}