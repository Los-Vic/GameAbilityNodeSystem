using NS;

namespace GAS.Logic
{
    [Node("Owner", "AbilitySystem/Value/Owner", ENodeType.Value , typeof(AbilityOwnerNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class AbilityOwnerNode:Node
    {
        [Port(EPortDirection.Output, typeof(GameUnit),"Unit")]
        public string OutPortUnit;
    }
    
    public sealed class AbilityOwnerNodeRunner:NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            if (node is not AbilityOwnerNode n)
            {
                graphRunner.Abort();
                return;
            }
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            graphRunner.SetOutPortVal(n.OutPortUnit, context.Ability.Owner);
        }
    }
}