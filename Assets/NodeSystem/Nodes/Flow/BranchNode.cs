using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Branch", "Default/FlowControl/Branch", ENodeCategory.FlowControl, ENodeNumsLimit.None, typeof(BranchNodeRunner))]
    public class BranchNode:NodeSystemNode
    {
        [Port(Direction.Input,typeof(ExecutePort))]
        public string InPortExec;

        [Port(Direction.Input,typeof(bool), "Condition")]
        public string InPortBool;

        [Port(Direction.Output, typeof(ExecutePort), "True")]
        public string OutPortTrueExec;
        
        [Port(Direction.Output, typeof(ExecutePort), "False")]
        public string OutPortFalseExec;
    }
    
    public class BranchNodeRunner:NodeSystemFlowNodeRunner
    {
        private BranchNode _node;
        private bool _condition;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
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