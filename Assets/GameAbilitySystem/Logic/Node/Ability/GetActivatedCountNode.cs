using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("GetActivatedCount", "Ability/Value/GetActivatedCount", ENodeFunctionType.Action, typeof(GetActivatedCountNodeNodeRunner), 
        CommonNodeCategory.Value, NodeScopeDefine.Ability)]
    public sealed class GetActivatedCountNode:Node
    {
        [Port(EPortDirection.Output, typeof(FP), "Count")]
        public string OutPortCount;
    }
    
    public sealed class GetActivatedCountNodeNodeRunner : FlowNodeRunner
    {
        private GetActivatedCountNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (GetActivatedCountNode)nodeAsset;
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