using NS;

namespace GAS.Logic
{
    [Node("CancelAbility", "AbilitySystem/Action/CancelAbility", ENodeFunctionType.Action, typeof(CancelAbilityNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem, 
        "Cancel ability if ability is in activated, will stop all tasks and cast processes.")]
    public sealed class CancelAbilityNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
    }
    
    public sealed class CancelAbilityNodeRunner : FlowNodeRunner
    {
        private CancelAbilityNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (CancelAbilityNode)context.Node;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            context.Ability.CancelAbility();
            Complete();
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}