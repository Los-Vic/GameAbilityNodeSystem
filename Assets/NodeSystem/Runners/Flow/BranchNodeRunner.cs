namespace NS
{
    public class BranchNodeRunner:NodeSystemFlowNodeRunner
    {
        private BranchNode _node;
        private NodeSystemGraphRunner _graphRunner;
        private bool _condition;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
           _node = (BranchNode)nodeAsset;
           _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            ExecuteDependentValNodes(_node.Id, _graphRunner);
            _condition = _graphRunner.GetInPortVal<bool>(_node.InPortBool);
            Complete();
        }

        public override string GetNextNode()
        {
            var outPortId =  _condition ? _node.OutPortTrueExec : _node.OutPortFalseExec;
            var port = _graphRunner.GraphAssetRuntimeData.GetPortById(outPortId);
            if (!port.IsConnected())
                return default;

            var connectPort = _graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}