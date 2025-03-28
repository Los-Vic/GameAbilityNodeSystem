namespace NS
{
    [Node("Reroute", "Common/Reroute", ENodeFunctionType.Value ,typeof(RerouteNodeRunner), CommonNodeCategory.Reroute)]
    public sealed class RerouteNode:Node
    {
        [Port(EPortDirection.Input, typeof(object))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(object))]
        public string OutPortVal;
    }
    
    public sealed class RerouteNodeRunner:NodeRunner
    {
        private RerouteNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (RerouteNode)nodeAsset;
        }

        public override void Execute()
        {
            var inVal = GraphRunner.GetInPortVal<object>(_node.InPortVal);
            GraphRunner.SetOutPortVal(_node.OutPortVal, inVal);
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}