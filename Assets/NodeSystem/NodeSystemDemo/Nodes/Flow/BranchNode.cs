using NS.Nodes;
using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Branch", "Default/FlowControl/Branch", (int)ENodeCategory.FlowControl, ENodeFunctionType.Flow, typeof(BranchFlowNodeRunner))]
    public class BranchNode:Node
    {
        [Port(Direction.Input,typeof(BaseFlowPort))]
        public string InPortExec;

        [Port(Direction.Input,typeof(bool), "Condition")]
        public string InPortBool;

        [Port(Direction.Output, typeof(BaseFlowPort), "True")]
        public string OutPortTrueExec;
        
        [Port(Direction.Output, typeof(BaseFlowPort), "False")]
        public string OutPortFalseExec;
    }
    
    public class BranchFlowNodeRunner:FlowNodeRunner
    {
        private BranchNode _node;
        private bool _condition;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (BranchNode)nodeAsset;
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(_node.Id);
            _condition = GraphRunner.GetInPortVal<bool>(_node.InPortBool);
            Complete();
        }

        public override string GetNextNode()
        {
            var outPortId =  _condition ? _node.OutPortTrueExec : _node.OutPortFalseExec;
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(outPortId);
            if (!port.IsConnected())
                return default;

            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}