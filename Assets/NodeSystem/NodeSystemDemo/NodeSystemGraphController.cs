using System.Collections.Generic;

namespace NS
{
    public class NodeSystemGraphController
    {
        private NodeSystem _system;
        private NodeGraphAsset _asset;

        private readonly List<NodeGraphRunner> _graphRunners = new();
        
        public void Init(NodeSystem system, NodeGraphAsset asset)
        {
            _system = system;
            _asset = asset;
        }

        public void DeInit()
        {
            foreach (var graphRunner in _graphRunners.ToArray())
            {
                _system.DestroyGraphRunner(graphRunner);
            }
            _graphRunners.Clear();
        }
        
        public void RunGraph(ENodeDemoPortalType demoPortalType, NodeDemoEntryParam param)
        {
            var runtimeData = _system.GetGraphRuntimeData(_asset);
            var nodeId = runtimeData.GetPortalNodeId(typeof(DemoPortalNode), (int)demoPortalType);
            if(nodeId == null)
                return;
            var graphRunner = _system.CreateGraphRunner();
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