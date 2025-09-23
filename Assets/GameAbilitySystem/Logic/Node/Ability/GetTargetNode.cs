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
        private bool _found;

        public override void Init(NodeGraphRunner graphRunner, Node node)
        {
            _found = false;
        }

        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (GetTargetNode)node;
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            
            _found = TargetQueryUtility.GetTargetFromAbility(context.Ability, n.TargetSingleCfg,
                out var target, n.IgnoreSelf);

            if(_found)
                graphRunner.SetOutPortVal(n.OutUnit, target);
            
            GameLogger.Log(_found
                ? $"Ability {context.Ability} found target {target}"
                : $"Ability {context.Ability} found target failed");
            graphRunner.Forward();   
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (GetTargetNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_found
                ? n.OutFlowPort
                : n.OutFlowPortFail);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
    
    public sealed class GetTargetsNodeRunner : FlowNodeRunner
    {
        private bool _found;

        public override void Init(NodeGraphRunner graphRunner, Node node)
        {
            _found = false;
        }

        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (GetTargetsNode)node;
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var targets = new List<GameUnit>();

            _found = TargetQueryUtility.GetTargetsFromAbility(context.Ability,
                n.TargetMultipleCfg, ref targets, n.IgnoreSelf);
            
            graphRunner.SetOutPortVal(n.OutUnitList, targets);
            GameLogger.Log($"Ability {context.Ability} found targets count {targets.Count}");
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (GetTargetsNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_found ? n.OutFlowPort: n.OutFlowPortFail);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}