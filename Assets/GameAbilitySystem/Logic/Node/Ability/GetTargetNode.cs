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
        
    }
    
    public sealed class GetTargetsNodeRunner : FlowNodeRunner
    {
        
    }
}