using MissQ;

namespace NS
{
    [Node("FP2Int", "Common/Value/FP2Int", ENodeFunctionType.Value , typeof(FP2IntNodeRunner), CommonNodeCategory.Value)]
    public sealed class FP2IntNode:Node
    {
        [Port(EPortDirection.Input, typeof(FP))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(int))]
        public string OutPortVal;
    }
    
    public sealed class FP2IntNodeRunner:NodeRunner
    {
        private FP2IntNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (FP2IntNode)context.Node;
        }

        public override void Execute()
        {
            var val = GraphRunner.GetInPortVal<FP>(_node.InPortVal);
            GraphRunner.SetOutPortVal(_node.OutPortVal, (int)val);
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
    
    [Node("FP2Float", "Common/Value/FP2Float", ENodeFunctionType.Value , typeof(FP2FloatNodeRunner), CommonNodeCategory.Value)]
    public sealed class FP2FloatNode:Node
    {
        [Port(EPortDirection.Input, typeof(FP))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public sealed class FP2FloatNodeRunner:NodeRunner
    {
        private FP2FloatNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (FP2FloatNode)context.Node;
        }
        
        public override void Execute()
        {
            var val = GraphRunner.GetInPortVal<FP>(_node.InPortVal);
            GraphRunner.SetOutPortVal(_node.OutPortVal, (float)val);
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}