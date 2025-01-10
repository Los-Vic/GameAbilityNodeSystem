using System;
using System.Collections.Generic;
using System.Linq;
using NS;

namespace GAS.Logic
{
    public class GameAbilityGraphRunnerContext : NodeGraphRunnerContext
    {
        public GameAbility Ability { get; set; }
    }
    
    public class GameAbilityGraphController
    {
        private GameAbilitySystem _system;
        private NodeGraphAsset _asset;
        private readonly GameAbilityGraphRunnerContext _context = new();

        private readonly List<NodeGraphRunner> _graphRunners = new();
        
        private readonly Dictionary<EGameEventPortal, string> _gameEventTypeNodeIdMap = new();
        private readonly Dictionary<Type, string> _portTypeNodeIdMap = new();

        internal List<EGameEventPortal> GetRegisteredGameEvents() => _gameEventTypeNodeIdMap.Keys.ToList();
        
        internal void Init(GameAbilitySystem system, NodeGraphAsset asset, GameAbility ability)
        {
            _system = system;
            _asset = asset;
            _context.Ability = ability;

            foreach (var node in asset.nodes)
            {
                if(!node.IsPortalNode())
                    continue;
                
                if (node is GameEventPortalNode eventNode)
                {
                    _gameEventTypeNodeIdMap.TryAdd(eventNode.nodeEventPortal, node.Id);
                }
                else
                {
                    _portTypeNodeIdMap.TryAdd(node.GetType(), node.Id);
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
            _system = null;
            _asset = null;
            _context.Ability = null;
        }
        
        internal NodeGraphRunner RunGraphGameEvent(EGameEventPortal eventPortalType, GameEventNodeParam param, Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null)
        {
            if (!_gameEventTypeNodeIdMap.TryGetValue(eventPortalType, out var nodeId))
            {
                NodeSystemLogger.LogError($"Not found {eventPortalType} node in {_asset.name}!");
                return null;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            _graphRunners.Add(graphRunner);
            graphRunner.Init(_system, _asset, nodeId, param, (runner, endType) =>
            {
                customOnRunGraphEnd?.Invoke(runner, endType);
                OnRunGraphEnd(runner, endType);
            }, _context);
            graphRunner.StartRunner();
            return graphRunner;
        }
        
        internal NodeGraphRunner RunGraph(Type portalNodeType, GameEventNodeParam param = null, Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null)
        {
            if (!_portTypeNodeIdMap.TryGetValue(portalNodeType, out var nodeId))
            {
                NodeSystemLogger.LogError($"Not found {portalNodeType} node in {_asset.name}!");
                return null;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            _graphRunners.Add(graphRunner);
            graphRunner.Init(_system, _asset, nodeId, param, (runner, endType) =>
            {
                customOnRunGraphEnd?.Invoke(runner, endType);
                OnRunGraphEnd(runner, endType);
            }, _context);
            graphRunner.StartRunner();
            return graphRunner;
        }
        
        private void OnRunGraphEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _system.NodeObjectFactory.DestroyGraphRunner(runner);
            _graphRunners.Remove(runner);
        }
    }
}