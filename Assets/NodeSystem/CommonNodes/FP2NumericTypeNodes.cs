using MissQ;

namespace NS
{
    [Node("FP2Int", "Common/Value/FP2Int", ENodeType.Value , typeof(FP2IntNodeRunner), CommonNodeCategory.Value)]
    public sealed class FP2IntNode:Node
    {
        [Port(EPortDirection.Input, typeof(FP))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(int))]
        public string OutPortVal;
    }
    
    public sealed class FP2IntNodeRunner:NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (FP2IntNode)node;
            var val = graphRunner.GetInPortVal<FP>(n.InPortVal);
            graphRunner.SetOutPortVal(n.OutPortVal, (int)val);
        }
    }
    
    [Node("FP2Float", "Common/Value/FP2Float", ENodeType.Value , typeof(FP2FloatNodeRunner), CommonNodeCategory.Value)]
    public sealed class FP2FloatNode:Node
    {
        [Port(EPortDirection.Input, typeof(FP))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public sealed class FP2FloatNodeRunner:NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (FP2FloatNode)node;
            var val = graphRunner.GetInPortVal<FP>(n.InPortVal);
            graphRunner.SetOutPortVal(n.OutPortVal, (float)val);
        }
    }
}