using System.Collections.Generic;
using System.Linq;
using NS;

namespace GAS.Logic
{
    public class GameAbilityGraphController
    {
        private GameAbilitySystem _system;
        private NodeGraphAsset _asset;

        private readonly List<NodeGraphRunner> _graphRunners = new();
        private readonly Dictionary<EGamePortal, string> _gameEventTypeIdMap = new();
        private readonly Dictionary<EDefaultPortal, string> _defaultEventIdMap = new();

        internal List<EGamePortal> GetRegisteredGameEvents() => _gameEventTypeIdMap.Keys.ToList();
        
        internal void Init(GameAbilitySystem system, NodeGraphAsset asset)
        {
            _system = system;
            _asset = asset;

            foreach (var node in asset.nodes)
            {
                if (node is GamePortalNode eventNode)
                {
                    _gameEventTypeIdMap.TryAdd(eventNode.NodePortal, node.Id);
                }
                else if (node is DefaultPortalNode defaultEventNode)
                {
                    _defaultEventIdMap.TryAdd(defaultEventNode.NodePortal, node.Id);
                }
            }
        }

        internal void UnInit()
        {
            foreach (var graphRunner in _graphRunners)
            {
                _system.NodeObjectFactory.DestroyGraphRunner(graphRunner);
            }
            _graphRunners.Clear();
        }
        
        internal void RunGraph(EGamePortal portalType, NodeActionStartParam param)
        {
            if (!_gameEventTypeIdMap.TryGetValue(portalType, out var nodeId))
            {
                NodeSystemLogger.LogError($"Not found {portalType} node in {_asset.name}!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            graphRunner.Init(_system, _asset, nodeId, param, OnRunGraphEnd);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        internal void RunGraph(EDefaultPortal portalType)
        {
            if (!_defaultEventIdMap.TryGetValue(portalType, out var nodeId))
            {
                NodeSystemLogger.Log($"Not found {portalType} node in {_asset.name}!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            graphRunner.Init(_system, _asset, nodeId, null, OnRunGraphEnd);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        private void OnRunGraphEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _graphRunners.Remove(runner);
        }
    }
}