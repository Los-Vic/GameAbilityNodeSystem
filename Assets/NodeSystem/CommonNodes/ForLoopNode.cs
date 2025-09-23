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
        private bool _started;
        private int _startIndex;
        private int _endIndex;
        private int _curIndex;
        private string _outPortId;
        
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (ForLoopNode)node;
            if (!_started)
            {
                graphRunner.EnterLoop(this, node);
                _started = true;
                _startIndex = graphRunner.GetInPortVal<int>(n.InStartIndex);
                _endIndex = graphRunner.GetInPortVal<int>(n.InEndIndex);
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
                _outPortId = n.OutCompleteExecPort;
                graphRunner.ExitLoop();
            }
            else
            {
                _outPortId = n.OutForEachExecPort;
            }

            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var outPort = graphRunner.GraphAssetRuntimeData.GetPortById(_outPortId);
            if (!outPort.IsConnected())
                return null;

            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(outPort.connectPortId);
            return connectPort.belongNodeId;
        }
        
        public override void OnReturnToPool()
        {
            _startIndex = 0;
            _curIndex = 0;
            _endIndex = 0;
            _started = false;
            base.OnReturnToPool();
        }
    }
}