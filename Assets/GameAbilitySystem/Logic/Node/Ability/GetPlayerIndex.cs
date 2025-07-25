using NS;

namespace GAS.Logic
{
    [Node("GetPlayerIndex", "AbilitySystem/Value/GetPlayerIndex", ENodeFunctionType.Action, typeof(GetPlayerIndexNodeRunner), 
        CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class GetPlayerIndexNode:Node
    {
        [Port(EPortDirection.Output, typeof(int), "PlayerIndex")]
        public string OutPortCount;
    }
    
    public sealed class GetPlayerIndexNodeRunner : FlowNodeRunner
    {
        private GetPlayerIndexNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (GetPlayerIndexNode)context.Node;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            context.Ability.System.GetRscFromHandler(context.Ability.Owner, out var owner);
            GraphRunner.SetOutPortVal(_node.OutPortCount, owner.PlayerIndex);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}