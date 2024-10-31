
namespace NS
{
    public class StartNodeRunner:NodeSystemFlowNodeRunner
    {
        private string _nextNode;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            var node = (StartNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void Execute(float dt = 0)
        {
            Complete();
        }

        public override string GetNextNode()
        {
            return _nextNode;
        }
        
    }
}