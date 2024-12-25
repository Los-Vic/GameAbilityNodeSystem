namespace NS
{
    [Node("Print","Common/Debug/Print", ENodeFunctionType.Flow, typeof(DebugPrintFlowNodeRunner), (int)ECommonNodeCategory.Debug)]
    public class DebugPrintNode:Node
    {
        [ExposedProp]
        public string Log;

        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        
    }
    
    public class DebugPrintFlowNodeRunner:FlowNodeRunner
    {
        private DebugPrintNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (DebugPrintNode)nodeAsset;
        }

        public override void Execute()
        {
            NodeSystemLogger.Log(_node.Log);
            Complete();
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return default;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}