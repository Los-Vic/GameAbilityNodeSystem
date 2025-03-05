using System;
using System.Collections.Generic;
using System.Linq;
using GameplayCommonLibrary;
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

        private readonly Dictionary<EGameEventType, string> _gameEventTypeNodeIdMap = new();
        private readonly Dictionary<Type, string> _portTypeNodeIdMap = new();

        internal List<EGameEventType> GetRegisteredGameEvents() => _gameEventTypeNodeIdMap.Keys.ToList();

        internal void Init(GameAbilitySystem system, NodeGraphAsset asset, GameAbility ability)
        {
            _system = system;
            _asset = asset;
            _context.Ability = ability;

            foreach (var node in asset.nodes)
            {
                if (!node.IsPortalNode())
                    continue;

                if (node is GameEventPortalNode eventNode)
                {
                    _gameEventTypeNodeIdMap.TryAdd(eventNode.nodeEventType, node.Id);
                }
                else
                {
                    _portTypeNodeIdMap.TryAdd(node.GetType(), node.Id);
                }
            }
        }

        internal void UnInit()
        {
            for(var i = _graphRunners.Count - 1; i >= 0; i--)
            {
                _system.NodeObjectFactory.DestroyGraphRunner(_graphRunners[i]);
            }

            _graphRunners.Clear();
            _system = null;
            _asset = null;
            _context.Ability = null;
        }

        internal NodeGraphRunner RunGraphGameEvent(EGameEventType eventTypeType, GameEventArg param,
            Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null,
            Action<NodeGraphRunner> customOnRunGraphDestroy = null)
        {
            if (!_gameEventTypeNodeIdMap.TryGetValue(eventTypeType, out var nodeId))
            {
                GameLogger.LogError($"Not found {eventTypeType} node in {_asset.name}!");
                return null;
            }

            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            _graphRunners.Add(graphRunner);
            graphRunner.Init(_system, _asset, nodeId, param, (runner, endType) =>
            {
                customOnRunGraphEnd?.Invoke(runner, endType);
                OnRunGraphEnd(runner, endType);
            }, (runner) =>
            {
                customOnRunGraphDestroy?.Invoke(runner);
                param.GetRefCountDisposableComponent().RemoveRefCount(graphRunner);
                OnRunGraphDestroy(runner);
            }, _context);
            
            param.GetRefCountDisposableComponent().AddRefCount(graphRunner);
            graphRunner.StartRunner();
            return graphRunner;
        }

        internal bool HasPortalNode(Type portalNodeType) => _portTypeNodeIdMap.ContainsKey(portalNodeType);

        internal NodeGraphRunner RunGraph(Type portalNodeType, GameEventArg param = null,
            Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null,
            Action<NodeGraphRunner> customOnRunGraphDestroy = null)
        {
            if (!_portTypeNodeIdMap.TryGetValue(portalNodeType, out var nodeId))
            {
                GameLogger.LogError($"Not found {portalNodeType} node in {_asset.name}!");
                return null;
            }

            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            _graphRunners.Add(graphRunner);
            graphRunner.Init(_system, _asset, nodeId, param, (runner, endType) =>
            {
                customOnRunGraphEnd?.Invoke(runner, endType);
                OnRunGraphEnd(runner, endType);
            }, (runner) =>
            {
                customOnRunGraphDestroy?.Invoke(runner);
                OnRunGraphDestroy(runner);
            }, _context);
            graphRunner.StartRunner();
            return graphRunner;
        }

        private void OnRunGraphEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _system.NodeObjectFactory.DestroyGraphRunner(runner);
        }

        private void OnRunGraphDestroy(NodeGraphRunner runner)
        {
            _graphRunners.Remove(runner);
        }
    }
}