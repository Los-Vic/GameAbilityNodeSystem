namespace NS
{
    [Node("Reroute", "Common/Reroute", ENodeType.Value ,typeof(RerouteNodeRunner), CommonNodeCategory.Reroute)]
    public sealed class RerouteNode:Node
    {
        [Port(EPortDirection.Input, typeof(object))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(object))]
        public string OutPortVal;
    }
    
    public sealed class RerouteNodeRunner:NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            if (node is not RerouteNode rerouteNode)
            {
                graphRunner.Abort();
                return;
            }
            var inVal = graphRunner.GetInPortVal<object>(rerouteNode.InPortVal);
            graphRunner.SetOutPortVal(rerouteNode.OutPortVal, inVal);
        }
    }
}