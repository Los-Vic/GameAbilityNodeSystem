using GameplayCommonLibrary;

namespace NS
{
    [Node("Print","Common/Debug/Print", ENodeFunctionType.Action, typeof(DebugPrintFlowNodeRunner), CommonNodeCategory.Debug)]
    public sealed class DebugPrintNode:Node
    {
        [Exposed]
        public string Log;

        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        
    }
    
    public sealed class DebugPrintFlowNodeRunner:FlowNodeRunner
    {
        private DebugPrintNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (DebugPrintNode)nodeAsset;
        }

        public override void Execute()
        {
            GameLogger.Log(_node.Log);
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
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}