using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NS
{
    [Node("Print","Default/Debug/Print", (int)ENodeCategory.ExecDebugInstant, typeof(DebugPrintFlowNodeRunner))]
    public class DebugPrintNode:Node
    {
        [ExposedProp]
        public string Log;

        [Port(Direction.Input, typeof(ExecutePort))]
        public string InPortExec;
        [Port(Direction.Output, typeof(ExecutePort))]
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
            Debug.Log(_node.Log);
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