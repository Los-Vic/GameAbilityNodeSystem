using UnityEngine;

namespace NS
{
    public class DebugPrintNodeRunner:NodeSystemFlowNodeRunner
    {
        private DebugPrintNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = (DebugPrintNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            Debug.Log(_node.Log);
            Complete();
        }

        public override string GetNextNode()
        {
            var port = _graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return default;
            var connectPort = _graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}