using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Reroute", "Common/Reroute", ENodeFunctionType.Value ,typeof(RerouteNodeRunner), (int)ECommonNodeCategory.Value)]
    public class RerouteNode:Node
    {
        [Port(Direction.Input, typeof(object))]
        public string InPortVal;

        [Port(Direction.Output, typeof(object))]
        public string OutPortVal;
    }
    
    public class RerouteNodeRunner:NodeRunner
    {
        private RerouteNode _node;
        private NodeGraphRunner _graphRunner;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = (RerouteNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute()
        {
            var inVal = _graphRunner.GetInPortVal<object>(_node.InPortVal);
            _graphRunner.SetOutPortVal(_node.OutPortVal, inVal);
        }
    }
}