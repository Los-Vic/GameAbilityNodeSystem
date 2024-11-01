
namespace NS
{
    public class EventNodeRunner:NodeSystemEventNodeRunner
    {
        private string _nextNode;
        private NodeSystemGraphRunner _runner;
        private EventNode _node;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = (EventNode)nodeAsset;
            _runner = graphRunner;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetUpEventParam(NodeSystemEventParamBase paramBase)
        {
            if (paramBase is not NodeEventParam param) 
                return;
            _runner.SetOutPortVal(_node.OutIntParam1, param.IntParam1);
            _runner.SetOutPortVal(_node.OutIntParam2, param.IntParam2);
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