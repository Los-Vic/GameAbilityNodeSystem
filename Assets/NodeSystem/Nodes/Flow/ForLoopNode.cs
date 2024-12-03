using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("ForLoop", "Default/FlowControl/ForLoop", (int)ENodeCategory.FlowControl, typeof(ForLoopFlowNodeRunner))]
    public class ForLoopNode:Node
    {
        [Port(Direction.Input, typeof(ExecutePort))]
        public string InExecPort;

        [Port(Direction.Input, typeof(int), "Start")]
        public string InStartIndex;
        
        [Port(Direction.Input, typeof(int), "End")]
        public string InEndIndex;

        [Port(Direction.Output, typeof(ExecutePort), "Completed")]
        public string OutCompleteExecPort;
        
        [Port(Direction.Output, typeof(ExecutePort),"ForEach")]
        public string OutForEachExecPort;
    }
    
    public class ForLoopFlowNodeRunner:FlowNodeRunner
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
    }
}