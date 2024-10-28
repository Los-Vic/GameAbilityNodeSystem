using NodeSystem.Nodes;
using UnityEngine;

namespace NodeSystem.Runners
{
    public class DelayNodeRunner:NodeSystemNodeRunner
    {
        private DelayNode _node;
        private NodeSystemGraphRunner _graphRunner;
        private float _delay;
        private float _elapsedTime;
        private bool _dependentNodesExecuted;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (DelayNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            Debug.Log($"Execute Delay Node, ElapsedTime[{_elapsedTime}]");
            _elapsedTime += dt;

            if (!_dependentNodesExecuted)
            {
                _dependentNodesExecuted = true;
                var nodeList = _graphRunner.GraphAssetRuntimeData.NodeValDependencyMap[_node.Id];
                for (var i = nodeList.Count - 1; i >= 0; i--)
                {
                    var runner = _graphRunner.NodeRunners[nodeList[i]];
                    runner.Execute();
                }

                var inPort = _graphRunner.GraphAssetRuntimeData.PortIdMap[_node.InIntPort];
                _delay = (int)_graphRunner.OutPortResultCached[inPort.connectPortId];
                Debug.Log($"Input Delay [{_delay}]");
            }

            if (_elapsedTime >= _delay)
            {
                IsNodeRunnerCompleted = true;
            }
        }

        public override string GetNextNode()
        {
            var port = _graphRunner.GraphAssetRuntimeData.PortIdMap[_node.OutPort];
            if(string.IsNullOrEmpty(port.connectPortId))
                return default;
            var connectPort = _graphRunner.GraphAssetRuntimeData.PortIdMap[port.connectPortId];
            return connectPort.belongNodeId;
        }
    }
}