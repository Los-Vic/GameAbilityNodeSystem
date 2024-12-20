using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("", "Common/Reroute", ENodeFunctionType.Reroute ,null, (int)ECommonNodeCategory.FlowControl)]
    public class RerouteNode:Node
    {
        [Port(Direction.Input, typeof(object))]
        public string InPortVal;

        [Port(Direction.Output, typeof(object))]
        public string OutPortVal;
    }
}