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
        private AbilityInstigatorNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AbilityInstigatorNode)nodeAsset;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            GraphRunner.SetOutPortVal(_node.OutPortUnit, context.Ability.Instigator);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}