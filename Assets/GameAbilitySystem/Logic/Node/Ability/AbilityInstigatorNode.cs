using NS;

namespace GAS.Logic
{
    [Node("Instigator", "Ability/Value/Instigator", ENodeFunctionType.Value , typeof(AbilityInstigatorNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.Ability)]
    public sealed class AbilityInstigatorNode:Node
    {
        [Port(EPortDirection.Output, typeof(GameUnit),"Unit")]
        public string OutPortUnit;
    }
    
    public sealed class AbilityInstigatorNodeRunner:NodeRunner
    {
        private AbilityInstigatorNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AbilityInstigatorNode)nodeAsset;
        }

        public override void Execute()
        {
            if (GraphRunner.Context is GameAbilityGraphRunnerContext context)
            {
                GraphRunner.SetOutPortVal(_node.OutPortUnit, context.Ability.Instigator);
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}