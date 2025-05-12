using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("GetAbilityLevelNode", "AbilitySystem/Value/GetAbilityLevelNode", ENodeFunctionType.Action, typeof(GetAbilityLevelNodeRunner), 
        CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class GetAbilityLevelNode:Node
    {
        [Port(EPortDirection.Output, typeof(FP), "Count")]
        public string OutPortCount;
    }
    
    public sealed class GetAbilityLevelNodeRunner : FlowNodeRunner
    {
        private GetAbilityLevelNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (GetAbilityLevelNode)nodeAsset;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            GraphRunner.SetOutPortVal(_node.OutPortCount, (FP)context.Ability.Lv);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}