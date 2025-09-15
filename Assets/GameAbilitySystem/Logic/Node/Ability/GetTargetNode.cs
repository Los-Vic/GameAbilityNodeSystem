using System.Collections.Generic;
using GCL;
using GAS.Logic.Target;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("GetTargetNode", "AbilitySystem/Action/GetTarget", ENodeFunctionType.Action, typeof(GetTargetNodeRunner),
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class GetTargetNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InFlowPort;

        [Exposed] 
        public bool IgnoreSelf;
        
        [Exposed]
        [SerializeReference]
        public TargetQuerySingleBase TargetSingleCfg;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort), "Found")]
        public string OutFlowPort;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort), "NoTarget")]
        public string OutFlowPortFail;

        [Port(EPortDirection.Output, typeof(GameUnit), "Target")]
        public string OutUnit;
    }
    
    [Node("GetTargetsNode", "AbilitySystem/Action/GetTargets", ENodeFunctionType.Action, typeof(GetTargetsNodeRunner),
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class GetTargetsNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InFlowPort;
        
        [Exposed] 
        public bool IgnoreSelf;
        
        [Exposed]
        [SerializeReference]
        public TargetQueryMultipleBase TargetMultipleCfg;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort), "Found")]
        public string OutFlowPort;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort), "NoTarget")]
        public string OutFlowPortFail;
        
        [Port(EPortDirection.Output, typeof(List<GameUnit>), "TargetList")]
        public string OutUnitList;
    }

    public sealed class GetTargetNodeRunner : FlowNodeRunner
    {
        private GetTargetNode _node;
        private bool _found;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (GetTargetNode)context.Node;
            _found = false;
        }

        public override void Execute()
        {
            base.Execute();
            
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            
            _found = TargetQueryUtility.GetTargetFromAbility(context.Ability, _node.TargetSingleCfg,
                out var target, _node.IgnoreSelf);

            if(_found)
                GraphRunner.SetOutPortVal(_node.OutUnit, target);
            
            GameLogger.Log(_found
                ? $"Ability {context.Ability} found target {target}"
                : $"Ability {context.Ability} found target failed");
            Complete();   
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_found
                ? _node.OutFlowPort
                : _node.OutFlowPortFail);
            if(!port.IsConnected())
                return null;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
        
        public override void OnReturnToPool()
        {
            _node = null;
            base.OnReturnToPool();
        }
    }
    
    public sealed class GetTargetsNodeRunner : FlowNodeRunner
    {
        private GetTargetsNode _node;
        private bool _found;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (GetTargetsNode)context.Node;
            _found = false;
        }

        public override void Execute()
        {
            base.Execute();
            
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var targets = new List<GameUnit>();

            _found = TargetQueryUtility.GetTargetsFromAbility(context.Ability,
                _node.TargetMultipleCfg, ref targets, _node.IgnoreSelf);
            
            GraphRunner.SetOutPortVal(_node.OutUnitList, targets);
            GameLogger.Log($"Ability {context.Ability} found targets count {targets.Count}");
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_found ? _node.OutFlowPort: _node.OutFlowPortFail);
            if(!port.IsConnected())
                return null;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
        
        public override void OnReturnToPool()
        {
            _node = null;
            base.OnReturnToPool();
        }
    }
}