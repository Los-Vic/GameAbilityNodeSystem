namespace NS
{
    [Node("ForLoop", "Common/FlowControl/ForLoop", ENodeFunctionType.Action, typeof(ForLoopFlowNodeRunner), CommonNodeCategory.FlowControl)]
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
    
    public sealed class ForLoopFlowNodeRunner:LoopNodeRunner
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
        
        public override void Execute()
        {
            base.Execute();
            if (!_started)
            {
                GraphRunner.EnterLoop(this);
                _started = true;
                _startIndex = GraphRunner.GetInPortVal<int>(_node.InStartIndex);
                _endIndex = GraphRunner.GetInPortVal<int>(_node.InEndIndex);
                _curIndex = _startIndex;
                _started = true;
            }

            _curIndex++;
            if (_curIndex > _endIndex)
            {
                IsLoopEnd = true;
            }
            
            if (IsLoopEnd)
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
            _startIndex = 0;
            _curIndex = 0;
            _endIndex = 0;
            _started = false;
            _node = null;
            base.OnReturnToPool();
        }
    }
}