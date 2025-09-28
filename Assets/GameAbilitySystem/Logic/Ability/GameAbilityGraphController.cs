using System;
using System.Collections.Generic;
using GCL;
using NS;

namespace GAS.Logic
{
    public class GameAbilityGraphRunnerContext : NodeGraphRunnerContext
    {
        public GameAbility Ability { get; set; }
    }

    public class GameAbilityGraphController:NodeGraphController
    {
        private GameAbilitySystem _system;
        private readonly GameAbilityGraphRunnerContext _context = new();

        internal void Init(GameAbilitySystem system, NodeGraphAsset asset, GameAbility ability)
        {
            _system = system;
            _context.Ability = ability;
            InitController(system, asset);
        }

        internal void UnInit()
        {
            UnInitController();
            _system = null;
            _context.Ability = null;
        }

        internal NodeGraphRunner RunGraphGameEvent(EGameEventType eventTypeType, GameEventArg param,
            Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null)
        {
            var nodeId = RuntimeData.GetEntryNodeId(typeof(GameEventEntryNode), (int)eventTypeType);
            if (nodeId == null)
            {
                GameLogger.LogError($"Not found {eventTypeType} node in {Asset.name}!");
                return null;
            }
      
            var graphRunner = CreateGraphRunner(nodeId, param, _context);
            
            if (customOnRunGraphEnd != null)
            {
                graphRunner.OnRunnerRunEnd += customOnRunGraphEnd;
            }
            graphRunner.OnRunnerRunEnd += (runner, endType) =>
            {
                _system.HandlerManagers.EventArgHandlerMgr.RemoveRefCount(param.Handler);
            };
            
            _system.HandlerManagers.EventArgHandlerMgr.AddRefCount(param.Handler);
            graphRunner.Start();
            return graphRunner;
        }

        internal NodeGraphRunner RunGraph(Type portalNodeType, GameEventArg param = null,
            Action<NodeGraphRunner, EGraphRunnerEnd> customOnRunGraphEnd = null)
        {
            var nodeId = RuntimeData.GetEntryNodeId(portalNodeType);
            if (nodeId == null)
            {
                GameLogger.LogError($"Not found {portalNodeType} node in {Asset.name}!");
                return null;
            }

            var graphRunner = CreateGraphRunner(nodeId, param, _context);

            if (customOnRunGraphEnd != null)
            {
                graphRunner.OnRunnerRunEnd += customOnRunGraphEnd;
            }
            
            graphRunner.Start();
            return graphRunner;
        }

        internal List<(int, string)> GetRegisteredGameEventNodePairs() =>
            RuntimeData.GetEntryNodePairList(typeof(GameEventEntryNode));
        
    }
}