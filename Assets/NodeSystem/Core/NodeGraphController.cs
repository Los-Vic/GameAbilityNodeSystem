using System;
using System.Collections.Generic;

namespace NS
{
    public class NodeGraphController
    {
        private NodeSystem _nodeSystem;
        private readonly List<NodeGraphRunner> _graphRunners = new();

        protected NodeGraphAsset Asset;
        protected GraphAssetRuntimeData RuntimeData;
        
        protected virtual void InitController(NodeSystem nodeSystem, NodeGraphAsset asset)
        {
            _nodeSystem = nodeSystem;
            Asset = asset;
            RuntimeData = _nodeSystem.GetGraphRuntimeData(asset);
        }

        protected virtual void UnInitController()
        {
            for(var i = _graphRunners.Count - 1; i >= 0; i--)
            {
                _nodeSystem.DestroyGraphRunner(_graphRunners[i]);
            }
            _graphRunners.Clear();
            _nodeSystem = null;
            Asset = null;
            RuntimeData = null;
        }

        protected virtual NodeGraphRunner CreateGraphRunner(string entryNodeId, IEntryParam entryParam, NodeGraphRunnerContext context = null)
        {
            var graphRunner = _nodeSystem.CreateGraphRunner();
            _graphRunners.Add(graphRunner);

            var initContext = new NodeGraphRunnerInitContext()
            {
                System = _nodeSystem,
                EntryParam = entryParam,
                Asset = Asset,
                Context = context,
                EntryNodeId = entryNodeId
            };
            graphRunner.Init(ref initContext);
            graphRunner.OnRunnerRunEnd += OnRunnerRunEnd;
            return graphRunner;
        }
        
        protected virtual void DestroyGraphRunner(NodeGraphRunner runner)
        {
            if (!_graphRunners.Contains(runner))
                return;
            _graphRunners.Remove(runner);
            _nodeSystem.DestroyGraphRunner(runner);
        }
        
        public bool HasEntryNode(Type portalNodeType) => RuntimeData.GetEntryNodeId(portalNodeType) != null;
        
        private void OnRunnerRunEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            DestroyGraphRunner(runner);
        }
    }
}