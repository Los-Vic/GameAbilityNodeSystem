using System.Collections.Generic;
using GAS.Logic.Target;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("GetTargetFromAbilityNode", "Ability/Action/GetTargetFromAbility", ENodeFunctionType.Action, typeof(GetTargetFromAbilityNodeRunner),
        CommonNodeCategory.Action, NodeScopeDefine.Ability)]
    public sealed class GetTargetFromAbilityNode:Node
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
    
    [Node("GetTargetsFromAbilityNode", "Ability/Action/GetTargetsFromAbility", ENodeFunctionType.Action, typeof(GetTargetsFromAbilityNodeRunner),
        CommonNodeCategory.Action, NodeScopeDefine.Ability)]
    public sealed class GetTargetsFromAbilityNode:Node
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

    public sealed class GetTargetFromAbilityNodeRunner : FlowNodeRunner
    {
        
    }
    
    public sealed class GetTargetsFromAbilityNodeRunner : FlowNodeRunner
    {
        
    }
}