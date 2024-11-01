using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Delay", "Default/NonInstantAction/Delay", ENodeCategory.ExecNonInstant, ENodeNumsLimit.None, typeof(DelayNodeRunner))]
    public class DelayNode:NodeSystemNode
    {
        [Port(Direction.Input, typeof(ExecutePort))]
        public string InPortExec;
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;

        [Port(Direction.Input, typeof(float), "Duration")]
        public string InPortFloat;
    }
    
    public class DelayNodeRunner:NodeSystemFlowNodeRunner
    {
        private DelayNode _node;
        private NodeSystemGraphRunner _graphRunner;
        private float _delay;
        private float _elapsedTime;
        private bool _started;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = (DelayNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Reset()
        {
            base.Reset();
            _started = false;
            _elapsedTime = 0;
        }

        public override void Execute(float dt = 0)
        {
            if (!_started)
            {
                ExecuteDependentValNodes(_node.Id, _graphRunner);
                _delay = _graphRunner.GetInPortVal<float>(_node.InPortFloat);
                NodeSystemLogger.Log($"Input Delay [{_delay}]");
                _started = true;
            }
            
            _elapsedTime += dt;
            if (_elapsedTime >= _delay)
            {
                Complete();
            }
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