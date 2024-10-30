using UnityEngine;

namespace NS
{
    public class StartNodeRunner:NodeSystemFlowNodeRunner
    {
        private string _nextNode;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            var node = (StartNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.PortIdMap[node.OutPortExec];
            if(string.IsNullOrEmpty(port.connectPortId))
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.PortIdMap[port.connectPortId];
            _nextNode = connectPort.belongNodeId;
        }

        public override void Execute(float dt = 0)
        {
            IsNodeRunnerCompleted = true;
        }

        public override string GetNextNode()
        {
            return _nextNode;
        }
        
    }
}