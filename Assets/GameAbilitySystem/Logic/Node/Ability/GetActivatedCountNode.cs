using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("GetActivatedCount", "AbilitySystem/Value/GetActivatedCount", ENodeType.Action, typeof(GetActivatedCountNodeRunner), 
        CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class GetActivatedCountNode:Node
    {
        [Port(EPortDirection.Output, typeof(FP), "Count")]
        public string OutPortCount;
    }
    
    public sealed class GetActivatedCountNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            if (node is not GetActivatedCountNode n)
            {
                graphRunner.Abort();
                return;
            }
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            graphRunner.SetOutPortVal(n.OutPortCount, (FP)context.Ability.ActivatedCount);
        }
    }
}