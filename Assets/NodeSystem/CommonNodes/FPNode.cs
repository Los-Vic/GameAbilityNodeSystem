using MissQ;

namespace NS
{
    [Node("FP", "Common/Value/FP", ENodeFunctionType.Value , typeof(FPNodeRunner), CommonNodeCategory.Value)]
    public sealed class FPNode:Node
    {
        [Exposed]
        public float Val;

        [Port(EPortDirection.Output, typeof(FP))]
        public string OutPortVal;
    }
    
    public sealed class FPNodeRunner:NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (FPNode)node;
            graphRunner.SetOutPortVal(n.OutPortVal, (FP)n.Val);
        }
    }
}