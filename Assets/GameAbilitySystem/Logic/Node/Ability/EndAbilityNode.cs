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