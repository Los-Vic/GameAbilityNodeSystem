namespace NS
{
    [Node("Int", "Common/LiteralValue/Int", ENodeFunctionType.Value , typeof(IntNodeRunner), CommonNodeCategory.Value)]
    public sealed class IntNode:Node
    {
        [ExposedProp]
        public int Val;

        [Port(EPortDirection.Output, typeof(int))]
        public string OutPortVal;
    }
    
    public sealed class IntNodeRunner:NodeRunner
    {
        private IntNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = ((IntNode)nodeAsset);
        }

        public override void Execute()
        {
            GraphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}