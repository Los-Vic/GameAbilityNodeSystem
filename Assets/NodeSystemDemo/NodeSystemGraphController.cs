using System.Collections.Generic;

namespace NS
{
    public class NodeSystemGraphController
    {
        private NodeSystem _system;
        private NodeSystemGraphAsset _asset;

        private readonly List<NodeSystemGraphRunner> _graphRunners = new();
        private readonly List<NodeSystemGraphRunner> _pendingDestroyGraphRunners = new();
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
            foreach (var graphRunner in _graphRunners)
            {
                _system.NodeObjectFactory.DestroyGraphRunner(graphRunner);
            }
            _graphRunners.Clear();
        }
        
        public void RunGraph(ENodeEventType eventType, NodeEventParam param)
        {
            if (!_eventTypeIdMap.TryGetValue(eventType, out var nodeId))
            {
                NodeSystemLogger.LogWarning($"Not found {eventType} node!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            graphRunner.Init(_system, _asset, nodeId, param);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        public void UpdateGraphs(float deltaTime)
        {
            foreach (var runner in _graphRunners)
            {
                runner.UpdateRunner(deltaTime);
                if (!runner.IsRunning())
                {
                    _pendingDestroyGraphRunners.Add(runner);
                }
            }

            foreach (var runner in _pendingDestroyGraphRunners)
            {
                _graphRunners.Remove(runner);
                _system.NodeObjectFactory.DestroyGraphRunner(runner);
            }
            _pendingDestroyGraphRunners.Clear();
        }
    }
}