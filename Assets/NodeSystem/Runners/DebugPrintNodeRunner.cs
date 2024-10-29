using NodeSystem.Core;
using NodeSystem.Nodes;
using UnityEngine;

namespace NodeSystem.Runners
{
    public class DebugPrintNodeRunner:NodeSystemFlowNodeRunner
    {
        private DebugPrintNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (DebugPrintNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            Debug.Log("Execute DebugPrintNode");
            Debug.Log(_node.Log);
            IsNodeRunnerCompleted = true;
        }

        public override string GetNextNode()
        {
            var port = _graphRunner.GraphAssetRuntimeData.PortIdMap[_node.OutPort];
            if(string.IsNullOrEmpty(port.connectPortId))
                return default;
            var connectPort = _graphRunner.GraphAssetRuntimeData.PortIdMap[port.connectPortId];
            return connectPort.belongNodeId;
        }
    }
}