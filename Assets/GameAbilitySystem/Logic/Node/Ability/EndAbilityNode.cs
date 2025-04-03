using NS;

namespace GAS.Logic
{
    [Node("EndAbility", "Ability/Action/EndAbility", ENodeFunctionType.Value, typeof(EndAbilityNodeNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.Ability, 
        "End ability if ability is in activated, that is either OnActivateAbility or OnActivateAbilityByEvent is running with tasks")]
    public sealed class EndAbilityNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
    }
    
    public sealed class EndAbilityNodeNodeRunner : FlowNodeRunner
    {
        private EndAbilityNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (EndAbilityNode)nodeAsset;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            context.Ability.EndAbility();
            Complete();
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}