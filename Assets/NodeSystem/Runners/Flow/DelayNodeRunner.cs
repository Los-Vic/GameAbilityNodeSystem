using UnityEngine;

namespace NS
{
    public class DelayNodeRunner:NodeSystemFlowNodeRunner
    {
        private DelayNode _node;
        private NodeSystemGraphRunner _graphRunner;
        private float _delay;
        private float _elapsedTime;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = (DelayNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            if (!DependentValNodesExecuted)
            {
                ExecuteDependentValNodes(_node.Id, _graphRunner);
                _delay = (float)_graphRunner.GetInPortVal(_node.InPortFloat);
                Debug.Log($"Input Delay [{_delay}]");
            }
            
            _elapsedTime += dt;
            if (_elapsedTime >= _delay)
            {
                IsNodeRunnerCompleted = true;
            }
        }

        public override string GetNextNode()
        {
            var port = _graphRunner.GraphAssetRuntimeData.PortIdMap[_node.OutPortExec];
            if(string.IsNullOrEmpty(port.connectPortId))
                return default;
            var connectPort = _graphRunner.GraphAssetRuntimeData.PortIdMap[port.connectPortId];
            return connectPort.belongNodeId;
        }
    }
}