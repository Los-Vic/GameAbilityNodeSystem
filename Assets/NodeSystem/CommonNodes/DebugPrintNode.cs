using Gameplay.Common;

namespace NS
{
    [Node("Print","Common/Debug/Print", ENodeType.Action, typeof(DebugPrintFlowNodeRunner), CommonNodeCategory.Debug)]
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
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (DebugPrintNode)node;
            GameLogger.Log(n.Log);
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var  n = (DebugPrintNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}