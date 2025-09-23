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
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            context.Ability.CancelAbility();
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (CancelAbilityNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}