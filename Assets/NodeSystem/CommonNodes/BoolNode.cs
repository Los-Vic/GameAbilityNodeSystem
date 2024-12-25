namespace NS
{
    [Node("Bool", "Common/LiteralValue/Bool", ENodeFunctionType.Value ,typeof(BoolNodeRunner), (int)ECommonNodeCategory.Value)]
    public class BoolNode : Node
    {
        [ExposedProp] public bool Val;

        [Port(EPortDirection.Output, typeof(bool))] 
        public string OutPortVal;
    }
    
    public class BoolNodeRunner:NodeRunner
    {
        private BoolNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = ((BoolNode)nodeAsset);
        }

        public override void Execute()
        {
            GraphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}