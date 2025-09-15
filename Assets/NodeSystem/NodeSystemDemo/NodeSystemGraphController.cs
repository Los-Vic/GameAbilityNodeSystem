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
            for (var i = _graphRunners.Count - 1; i >= 0; i--)
            {
                _system.DestroyGraphRunner(_graphRunners[i]);
            }
            _graphRunners.Clear();
        }
        
        public void RunGraph(ENodeDemoEntryType demoEntryType, NodeDemoEntryParam param)
        {
            var runtimeData = _system.GetGraphRuntimeData(_asset);
            var nodeId = runtimeData.GetEntryNodeId(typeof(DemoPortalNode), (int)demoEntryType);
            if(nodeId == null)
                return;
            var graphRunner = _system.CreateGraphRunner();
            graphRunner.OnRunnerRunEnd += OnGraphRunEnd;
            var initParam = new NodeGraphRunnerInitContext()
            {
                System = _system,
                EntryParam = param,
                Asset = _asset,
                EntryNodeId = nodeId
            };
            graphRunner.Init(ref initParam);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        private void OnGraphRunEnd(NodeGraphRunner runner, EGraphRunnerEnd type)
        {
            _system.DestroyGraphRunner(runner);
            _graphRunners.Remove(runner);
        }
    }
}