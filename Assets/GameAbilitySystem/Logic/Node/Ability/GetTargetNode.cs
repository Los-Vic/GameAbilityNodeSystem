using System.Collections.Generic;
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
        [SerializeReference]
        public TargetSelectSingleBase TargetSingleCfg;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutFlowPort;

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
        [SerializeReference]
        public TargetSelectMultipleBase TargetMultipleCfg;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutFlowPort;
        
        [Port(EPortDirection.Output, typeof(List<GameUnit>), "TargetList")]
        public string OutUnitList;
    }

    public sealed class GetTargetNodeRunner : FlowNodeRunner
    {
        private GetTargetNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (GetTargetNode)nodeAsset;
        }

        public override void Execute()
        {
            base.Execute();
            
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            if (context.Ability.System.TargetSearcher.GetTargetFromAbility(context.Ability, _node.TargetSingleCfg,
                    out var target))
            {
                GraphRunner.SetOutPortVal(_node.OutUnit, target);
                Complete();   
            }
            else
            {
                Abort();
            }
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutFlowPort);
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
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (GetTargetsNode)nodeAsset;
        }

        public override void Execute()
        {
            base.Execute();
            
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var targets = new List<GameUnit>();
            if (context.Ability.System.TargetSearcher.GetTargetsFromAbility(context.Ability, _node.TargetMultipleCfg,
                   ref targets))
            {
                GraphRunner.SetOutPortVal(_node.OutUnitList, targets);
                Complete();   
            }
            else
            {
                Abort();
            }
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutFlowPort);
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