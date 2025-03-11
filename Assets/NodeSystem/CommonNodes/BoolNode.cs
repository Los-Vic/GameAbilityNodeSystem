namespace NS
{
    [Node("Bool", "Common/Value/Bool", ENodeFunctionType.Value ,typeof(BoolNodeRunner), CommonNodeCategory.Value)]
    public sealed class BoolNode : Node
    {
        [Exposed] public bool Val;

        [Port(EPortDirection.Output, typeof(bool))] 
        public string OutPortVal;
    }
    
    public sealed class BoolNodeRunner:NodeRunner
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

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}