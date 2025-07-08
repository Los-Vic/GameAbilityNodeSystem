using System;
using System.Collections.Generic;
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
        private GraphAssetRuntimeData _runtimeData;
        
        private readonly List<NodeGraphRunner> _graphRunners = new();

        internal void Init(GameAbilitySystem system, NodeGraphAsset asset, GameAbility ability)
        {
            _system = system;
            _asset = asset;
            _context.Ability = ability;
            _runtimeData = _system.GetGraphRuntimeData(asset);
        }

        internal void UnInit()
        {
            for(var i = _graphRunners.Count - 1; i >= 0; i--)
            {
                _system.DestroyGraphRunner(_graphRunners[i]);
            }

            _graphRunners.Clear();
            _system = null;
            _asset = null;
            _context.Ability = null;
        }

        internal NodeGraphRunner RunGraphGameEvent(EGameEventType eventTypeType, GameEventArg param,
            Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null)
        {
            var nodeId = _runtimeData.GetEntryNodeId(typeof(GameEventEntryNode), (int)eventTypeType);
            if (nodeId == null)
            {
                GameLogger.LogError($"Not found {eventTypeType} node in {_asset.name}!");
                return null;
            }
      
            var graphRunner = _system.CreateGraphRunner();
            _graphRunners.Add(graphRunner);
            
            if (customOnRunGraphEnd != null)
            {
                graphRunner.OnRunnerRunEnd += customOnRunGraphEnd;
            }
            graphRunner.OnRunnerRunEnd += (runner, endType) =>
            {
                _system.GameEventSubsystem.GameEventRscMgr.RemoveRefCount(param.Handler);
                OnRunGraphEnd(runner, endType);
            };
            
            graphRunner.Init(_system, _asset, nodeId, param, _context);
            
            _system.GameEventSubsystem.GameEventRscMgr.AddRefCount(param.Handler);
            graphRunner.StartRunner();
            return graphRunner;
        }

        internal NodeGraphRunner RunGraph(Type portalNodeType, GameEventArg param = null,
            Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null)
        {
            var nodeId = _runtimeData.GetEntryNodeId(portalNodeType);
            if (nodeId == null)
            {
                GameLogger.LogError($"Not found {portalNodeType} node in {_asset.name}!");
                return null;
            }

            var graphRunner = _system.CreateGraphRunner();
            _graphRunners.Add(graphRunner);

            if (customOnRunGraphEnd != null)
            {
                graphRunner.OnRunnerRunEnd += customOnRunGraphEnd;
            }
            graphRunner.OnRunnerRunEnd += OnRunGraphEnd;

            graphRunner.Init(_system, _asset, nodeId, param, _context);
            graphRunner.StartRunner();
            return graphRunner;
        }
        
        internal List<(int, string)> GetRegisteredGameEventNodePairs()=>_runtimeData.GetEntryNodePairList(typeof(GameEventEntryNode));

        internal bool HasEntryNode(Type portalNodeType) => _runtimeData.GetEntryNodeId(portalNodeType) != null;
        
        private void OnRunGraphEnd(NodeGraphRunner runner, EGraphRunnerEnd endType)
        {
            _system.DestroyGraphRunner(runner);
            _graphRunners.Remove(runner);
        }
    }
}