namespace NS
{
    [Node("Branch", "Common/FlowControl/Branch", ENodeType.Action, typeof(BranchFlowNodeRunner), CommonNodeCategory.FlowControl)]
    public sealed class BranchNode:Node
    {
        [Port(EPortDirection.Input,typeof(BaseFlowPort))]
        public string InPortExec;

        [Port(EPortDirection.Input,typeof(bool), "Condition")]
        public string InPortBool;

        [Port(EPortDirection.Output, typeof(BaseFlowPort), "True")]
        public string OutPortTrueExec;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort), "False")]
        public string OutPortFalseExec;
    }
    
    public sealed class BranchFlowNodeRunner:FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner,  Node node)
        {
            base.Execute(graphRunner, node);
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (BranchNode)node;
            var condition = graphRunner.GetInPortVal<bool>(n.InPortBool);
            var outPortId =  condition ? n.OutPortTrueExec : n.OutPortFalseExec;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(outPortId);
            if (!port.IsConnected())
                return null;

            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}