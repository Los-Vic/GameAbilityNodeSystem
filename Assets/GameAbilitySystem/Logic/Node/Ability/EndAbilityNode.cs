using NS;

namespace GAS.Logic
{
    [Node("EndAbility", "AbilitySystem/Action/EndAbility", ENodeFunctionType.Value, typeof(EndAbilityNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem, 
        "End ability will kill ability and remove it from owner")]
    public sealed class EndAbilityNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
    }
    
    public sealed class EndAbilityNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            context.Ability.EndAbility();
            graphRunner.Forward();
        }
    }
}