using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("GetAbilityLevelNode", "AbilitySystem/Value/GetAbilityLevelNode", ENodeType.Action, typeof(GetAbilityLevelNodeRunner), 
        CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class GetAbilityLevelNode:Node
    {
        [Port(EPortDirection.Output, typeof(FP), "Count")]
        public string OutPortCount;
    }
    
    public sealed class GetAbilityLevelNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var n = (GetAbilityLevelNode)node;
            graphRunner.SetOutPortVal(n.OutPortCount, (FP)context.Ability.Lv);
        }
    }
}