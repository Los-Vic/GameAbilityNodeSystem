using NodeSystem.Nodes;
using UnityEngine;

namespace NodeSystem.Runners
{
    public class StartNodeRunner:NodeSystemNodeRunner
    {
        private string _nextNode;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            var node = (StartNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.PortIdMap[node.OutPort];
            if(string.IsNullOrEmpty(port.connectPortId))
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.PortIdMap[port.connectPortId];
            _nextNode = connectPort.belongNodeId;
        }

        public override void Execute(float dt = 0)
        {
            Debug.Log("Execute StartNodeRunner");
            IsNodeRunnerCompleted = true;
        }

        public override string GetNextNode()
        {
            return _nextNode;
        }
    }
}