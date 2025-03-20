using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("GrantEffectForUnit", "Ability/Action/GrantEffectForUnit", ENodeFunctionType.Action, typeof(GrantEffectNodeRunner),
        CommonNodeCategory.Action, NodeScopeDefine.Ability)]
    public sealed class GrantEffectForUnitNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InFlowPort;

        [Port(EPortDirection.Input, typeof(GameUnit), "Unit")]
        public string InUnitPort;

        [Port(EPortDirection.Input, typeof(FP), "SignalVal1")]
        public string SignalVal1;
        [Port(EPortDirection.Input, typeof(FP), "SignalVal2")]
        public string SignalVal2;
        [Port(EPortDirection.Input, typeof(FP), "SignalVal3")]
        public string SignalVal3;
        
        [Exposed] 
        public EffectAsset Effect;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutFlowPort;
    }
    
    //todo:
    public sealed class GrantEffectNodeRunner : FlowNodeRunner
    {
        
    }
}