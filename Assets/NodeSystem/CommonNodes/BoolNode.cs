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

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (BoolNode)context.Node;
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