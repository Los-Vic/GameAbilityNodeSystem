namespace NS
{
    [Node("Float", "Common/LiteralValue/Float", ENodeFunctionType.Value, typeof(FloatNodeRunner), CommonNodeCategory.Value)]
    public sealed class FloatNode:Node
    {
        [ExposedProp]
        public float Val;

        [Port(EPortDirection.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public sealed class FloatNodeRunner:NodeRunner
    {
        private FloatNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = ((FloatNode)nodeAsset);
        }

        public override void Execute()
        {
            GraphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}