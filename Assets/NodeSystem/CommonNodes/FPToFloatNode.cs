using MissQ;

namespace NS
{
    [Node("FPToFloat", "Common/LiteralValue/FPToFloat",ENodeFunctionType.Value ,  typeof(FPToFloatNodeRunner), CommonNodeCategory.Value )]
    public sealed class FPToFloatNode:Node
    {
        [Port(EPortDirection.Input, typeof(FP))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public sealed class FPToFloatNodeRunner:NodeRunner
    {
        private FPToFloatNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (FPToFloatNode)nodeAsset;
        }

        public override void Execute()
        {
            var inVal = GraphRunner.GetInPortVal<int>(_node.InPortVal);
            var floatVal = (float)inVal;
            GraphRunner.SetOutPortVal(_node.OutPortVal, floatVal);
        }
    }
}