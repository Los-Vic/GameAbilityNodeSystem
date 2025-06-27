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
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (GetPlayerIndexNode)nodeAsset;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            GraphRunner.SetOutPortVal(_node.OutPortCount, context.Ability.Owner.PlayerIndex);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}