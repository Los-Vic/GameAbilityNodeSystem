using NS;

namespace GAS.Logic
{
    [Node("Owner", "AbilitySystem/Value/Owner", ENodeFunctionType.Value , typeof(AbilityOwnerNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class AbilityOwnerNode:Node
    {
        [Port(EPortDirection.Output, typeof(GameUnit),"Unit")]
        public string OutPortUnit;
    }
    
    public sealed class AbilityOwnerNodeRunner:NodeRunner
    {
        private AbilityOwnerNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (AbilityOwnerNode)context.Node;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            GraphRunner.SetOutPortVal(_node.OutPortUnit, context.Ability.Owner);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}