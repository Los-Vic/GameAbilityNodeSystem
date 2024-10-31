namespace NS
{
    public class ForLoopNodeRunner:NodeSystemFlowNodeRunner
    {
        private ForLoopNode _node;
        private NodeSystemGraphRunner _graphRunner;
        private bool _started;
        private int _startIndex;
        private int _endIndex;
        private int _curIndex;
        private string _outPortId;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
           _node = (ForLoopNode)nodeAsset;
           _graphRunner = graphRunner;
        }

        public override void Reset()
        {
            base.Reset();
            _startIndex = 0;
            _curIndex = 0;
            _endIndex = 0;
            _started = false;
        }

        public override void Execute(float dt = 0)
        {
            if (!_started)
            {
                ExecuteDependentValNodes(_node.Id, _graphRunner);
                _graphRunner.EnterLoop(_node.Id);
                _started = true;
                _startIndex = _graphRunner.GetInPortVal<int>(_node.InStartIndex);
                _endIndex = _graphRunner.GetInPortVal<int>(_node.InEndIndex);
                _curIndex = _startIndex;
                _started = true;
            }

            _curIndex++;
            if (_curIndex > _endIndex)
            {
                _outPortId = _node.OutCompleteExecPort;
                _graphRunner.ExitLoop();
            }
            else
            {
                _outPortId = _node.OutForEachExecPort;
            }

            Complete();
        }

        public override string GetNextNode()
        {
            var outPort = _graphRunner.GraphAssetRuntimeData.GetPortById(_outPortId);
            if (!outPort.IsConnected())
                return default;

            var connectPort = _graphRunner.GraphAssetRuntimeData.GetPortById(outPort.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}