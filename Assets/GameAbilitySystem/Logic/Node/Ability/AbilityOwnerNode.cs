using NS;

namespace GAS.Logic
{
    [Node("AbilityOwner", "Ability/Value/AbilityOwner", ENodeFunctionType.Value , typeof(AbilityOwnerNodeRunner), CommonNodeCategory.Value)]
    public sealed class AbilityOwnerNode:Node
    {
        [Port(EPortDirection.Output, typeof(GameUnit),"Unit")]
        public string OutPortUnit;
    }
    
    public sealed class AbilityOwnerNodeRunner:NodeRunner
    {
        private AbilityOwnerNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AbilityOwnerNode)nodeAsset;
        }

        public override void Execute()
        {
            if (GraphRunner.Context is GameAbilityGraphRunnerContext context)
            {
                GraphRunner.SetOutPortVal(_node.OutPortUnit, context.Ability.Owner);
            }
        }
    }
}