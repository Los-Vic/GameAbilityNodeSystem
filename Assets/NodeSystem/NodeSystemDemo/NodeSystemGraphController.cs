using System.Collections.Generic;

namespace NS
{
    public class NodeSystemGraphController
    {
        private NodeSystem _system;
        private NodeGraphAsset _asset;

        private readonly List<NodeGraphRunner> _graphRunners = new();
        private readonly Dictionary<ENodeDemoPortalType, string> _portalTypeIdMap = new();
        
        public void Init(NodeSystem system, NodeGraphAsset asset)
        {
            _system = system;
            _asset = asset;

            foreach (var node in asset.nodes)
            {
                if(node is not DemoPortalNode portalNode)
                    continue;

                _portalTypeIdMap.TryAdd(portalNode.nodeDemoPortal, node.Id);
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
        
        public void RunGraph(ENodeDemoPortalType demoPortalType, NodeDemoPortalParam param)
        {
            if (!_portalTypeIdMap.TryGetValue(demoPortalType, out var nodeId))
            {
                NodeSystemLogger.LogWarning($"not found {demoPortalType} node!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            graphRunner.Init(_system, _asset, nodeId, param, OnGraphRunEnd);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        private void OnGraphRunEnd(NodeGraphRunner runner, EGraphRunnerEnd type)
        {
            _graphRunners.Remove(runner);
        }
    }
}