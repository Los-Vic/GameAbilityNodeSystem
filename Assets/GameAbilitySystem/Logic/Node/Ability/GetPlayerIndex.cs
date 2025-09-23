using GCL;
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
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var n = (GetPlayerIndexNode)node;
            if (Singleton<HandlerMgr<GameUnit>>.Instance.DeRef(context.Ability.Owner, out var owner))
            {
                graphRunner.SetOutPortVal(n.OutPortCount, owner.PlayerIndex);
            }
        }
    }
}