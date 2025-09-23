using NS;

namespace GAS.Logic
{
    [Node("Instigator", "AbilitySystem/Value/Instigator", ENodeFunctionType.Value , typeof(AbilityInstigatorNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class AbilityInstigatorNode:Node
    {
        [Port(EPortDirection.Output, typeof(GameUnit),"Unit")]
        public string OutPortUnit;
    }
    
    public sealed class AbilityInstigatorNodeRunner:NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            if (node is not AbilityInstigatorNode n)
            {
                graphRunner.Abort();
                return;
            }
            
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            graphRunner.SetOutPortVal(n.OutPortUnit, context.Ability.Instigator);
        }
    }
}