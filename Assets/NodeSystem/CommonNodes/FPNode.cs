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
        private FPNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (FPNode)context.Node;
        }
        
        public override void Execute()
        {
            GraphRunner.SetOutPortVal(_node.OutPortVal, (FP)_node.Val);
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}