namespace NS
{
    [Node("IntToFloat", "Common/LiteralValue/IntToFloat",ENodeFunctionType.Value ,  typeof(IntToFloatNodeRunner), (int)ECommonNodeCategory.Value )]
    public class IntToFloatNode:Node
    {
        [Port(EPortDirection.Input, typeof(int))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public class IntToFloatNodeRunner:NodeRunner
    {
        private IntToFloatNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (IntToFloatNode)nodeAsset;
        }

        public override void Execute()
        {
            var inVal = GraphRunner.GetInPortVal<int>(_node.InPortVal);
            var floatVal = (float)inVal;
            GraphRunner.SetOutPortVal(_node.OutPortVal, floatVal);
        }
    }
}