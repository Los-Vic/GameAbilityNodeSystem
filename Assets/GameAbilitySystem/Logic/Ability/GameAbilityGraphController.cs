﻿using System;
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
        private readonly Dictionary<EGamePortal, string> _gameEventTypeNodeIdMap = new();
        private readonly Dictionary<Type, string> _portTypeNodeIdMap = new();

        internal List<EGamePortal> GetRegisteredGameEvents() => _gameEventTypeNodeIdMap.Keys.ToList();
        
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
                    _gameEventTypeNodeIdMap.TryAdd(eventNode.NodePortal, node.Id);
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
        
        internal void RunGraphGameEvent(EGamePortal portalType, GameEventNodeParam param)
        {
            if (!_gameEventTypeNodeIdMap.TryGetValue(portalType, out var nodeId))
            {
                NodeSystemLogger.LogError($"Not found {portalType} node in {_asset.name}!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            _graphRunners.Add(graphRunner);
            graphRunner.Init(_system, _asset, nodeId, param, OnRunGraphEnd, _context);
            graphRunner.StartRunner();
        }
        
        internal void RunGraph(Type portalNodeType, GameEventNodeParam param = null)
        {
            if (!_portTypeNodeIdMap.TryGetValue(portalNodeType, out var nodeId))
            {
                NodeSystemLogger.LogError($"Not found {portalNodeType} node in {_asset.name}!");
                return;
            }
            var graphRunner = _system.NodeObjectFactory.CreateGraphRunner();
            _graphRunners.Add(graphRunner);
            graphRunner.Init(_system, _asset, nodeId, param, OnRunGraphEnd, _context);
            graphRunner.StartRunner();
           
        }

        private void OnRunGraphEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _system.NodeObjectFactory.DestroyGraphRunner(runner);
            _graphRunners.Remove(runner);
        }
    }
}