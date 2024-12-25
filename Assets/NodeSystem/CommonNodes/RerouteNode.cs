namespace NS
{
    [Node("Reroute", "Common/Reroute", ENodeFunctionType.Value ,typeof(RerouteNodeRunner), (int)ECommonNodeCategory.Value)]
    public class RerouteNode:Node
    {
        [Port(EPortDirection.Input, typeof(object))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(object))]
        public string OutPortVal;
    }
    
    public class RerouteNodeRunner:NodeRunner
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
    }
}