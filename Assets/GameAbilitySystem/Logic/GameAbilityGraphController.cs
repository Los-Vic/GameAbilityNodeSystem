using System.Collections.Generic;
using System.Linq;
using NS;

namespace GAS.Logic
{
    public class GameAbilityGraphController
    {
        private GameAbilitySystem _system;
        private NodeSystemGraphAsset _asset;

        private readonly List<NodeSystemGraphRunner> _graphRunners = new();
        private readonly Dictionary<EGameEvent, string> _gameEventTypeIdMap = new();
        private readonly Dictionary<EDefaultEvent, string> _defaultEventIdMap = new();

        internal List<EGameEvent> GetRegisteredGameEvents() => _gameEventTypeIdMap.Keys.ToList();
        
        internal void Init(GameAbilitySystem system, NodeSystemGraphAsset asset)
        {
            _system = system;
            _asset = asset;

            foreach (var node in asset.nodes)
            {
                if (node is GameEventNode eventNode)
                {
                    _gameEventTypeIdMap.TryAdd(eventNode.NodeEvent, node.Id);
                }
                else if (node is DefaultEventNode defaultEventNode)
                {
                    _defaultEventIdMap.TryAdd(defaultEventNode.NodeEvent, node.Id);
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
        
        internal void RunGraph(EGameEvent eventType, NodeEventParam param)
        {
            if (!_gameEventTypeIdMap.TryGetValue(eventType, out var nodeId))
            {
                NodeSystemLogger.LogWarning($"Not found {eventType} node!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            graphRunner.Init(_system, _asset, nodeId, param, OnRunGraphEnd);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        internal void RunGraph(EDefaultEvent eventType)
        {
            if (!_defaultEventIdMap.TryGetValue(eventType, out var nodeId))
            {
                NodeSystemLogger.LogWarning($"Not found {eventType} node!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            graphRunner.Init(_system, _asset, nodeId, null, OnRunGraphEnd);
            graphRunner.StartRunner();
            _graphRunners.Add(graphRunner);
        }

        private void OnRunGraphEnd(NodeSystemGraphRunner runner, EGraphRunnerEnd endType)
        {
            _graphRunners.Remove(runner);
        }
    }
}