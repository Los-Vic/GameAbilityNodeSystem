using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("GetActivatedCount", "AbilitySystem/Value/GetActivatedCount", ENodeFunctionType.Action, typeof(GetActivatedCountNodeRunner), 
        CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class GetActivatedCountNode:Node
    {
        [Port(EPortDirection.Output, typeof(FP), "Count")]
        public string OutPortCount;
    }
    
    public sealed class GetActivatedCountNodeRunner : FlowNodeRunner
    {
        private GetActivatedCountNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (GetActivatedCountNode)context.Node;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            GraphRunner.SetOutPortVal(_node.OutPortCount, (FP)context.Ability.ActivatedCount);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}