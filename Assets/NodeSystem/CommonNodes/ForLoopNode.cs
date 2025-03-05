namespace NS
{
    [Node("ForLoop", "Common/FlowControl/ForLoop", ENodeFunctionType.Flow, typeof(ForLoopFlowNodeRunner), CommonNodeCategory.FlowControl)]
    public sealed class ForLoopNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InExecPort;

        [Port(EPortDirection.Input, typeof(int), "Start")]
        public string InStartIndex;
        
        [Port(EPortDirection.Input, typeof(int), "End")]
        public string InEndIndex;

        [Port(EPortDirection.Output, typeof(BaseFlowPort), "Completed")]
        public string OutCompleteExecPort;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort),"ForEach")]
        public string OutForEachExecPort;
    }
    
    public sealed class ForLoopFlowNodeRunner:FlowNodeRunner
    {
        private ForLoopNode _node;
        private bool _started;
        private int _startIndex;
        private int _endIndex;
        private int _curIndex;
        private string _outPortId;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ForLoopNode)nodeAsset;
        }

        public override void Reset()
        {
            base.Reset();
            _startIndex = 0;
            _curIndex = 0;
            _endIndex = 0;
            _started = false;
        }

        public override void Execute()
        {
            if (!_started)
            {
                ExecuteDependentValNodes(_node.Id);
                GraphRunner.EnterLoop(_node.Id);
                _started = true;
                _startIndex = GraphRunner.GetInPortVal<int>(_node.InStartIndex);
                _endIndex = GraphRunner.GetInPortVal<int>(_node.InEndIndex);
                _curIndex = _startIndex;
                _started = true;
            }

            _curIndex++;
            if (_curIndex > _endIndex)
            {
                _outPortId = _node.OutCompleteExecPort;
                GraphRunner.ExitLoop();
            }
            else
            {
                _outPortId = _node.OutForEachExecPort;
            }

            Complete();
        }

        public override string GetNextNode()
        {
            var outPort = GraphRunner.GraphAssetRuntimeData.GetPortById(_outPortId);
            if (!outPort.IsConnected())
                return default;

            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(outPort.connectPortId);
            return connectPort.belongNodeId;
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}