using System.Collections.Generic;

namespace NS
{
    public class NodeSystemGraphController
    {
        private NodeSystem _system;
        private NodeSystemGraphAsset _asset;

        private readonly List<NodeSystemGraphRunner> _graphRunners = new();
        private readonly Dictionary<ENodeEventType, string> _eventTypeIdMap = new();
        
        public void Init(NodeSystem system, NodeSystemGraphAsset asset)
        {
            _system = system;
            _asset = asset;

            foreach (var node in asset.nodes)
            {
                if(node is not EventNode eventNode)
                    continue;

                _eventTypeIdMap.TryAdd(eventNode.NodeEvent, node.Id);
            }
        }

        public void DeInit()
        {
            foreach (var graphRunner in _graphRunners.ToArray())
            {
                _system.NodeObjectFactory.DestroyGraphRunner(graphRunner);
            }
            _graphRunners.Clear();
        }
        
        public void RunGraph(ENodeEventType eventType, NodeEventParam param)
        {
            if (!_eventTypeIdMap.TryGetValue(eventType, out var nodeId))
            {
                NodeSystemLogger.LogWarning($"not found {eventType} node!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            graphRunner.Init(_system, _asset, nodeId, param, OnGraphRunEnd);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        private void OnGraphRunEnd(NodeSystemGraphRunner runner, EGraphRunnerEnd type)
        {
            _graphRunners.Remove(runner);
        }
    }
}