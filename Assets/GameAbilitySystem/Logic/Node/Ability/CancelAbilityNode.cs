using NS;

namespace GAS.Logic
{
    [Node("CancelAbility", "Ability/Exec/CancelAbility", ENodeFunctionType.Flow, typeof(CancelAbilityNodeNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.Ability, 
        "Cancel ability if ability is in activated, that is either OnActivateAbility or OnActivateAbilityByEvent is running with tasks")]
    public sealed class CancelAbilityNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
    }
    
    public sealed class CancelAbilityNodeNodeRunner : FlowNodeRunner
    {
        private CancelAbilityNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (CancelAbilityNode)nodeAsset;
        }

        public override void Execute()
        {
            if (GraphRunner.Context is GameAbilityGraphRunnerContext context)
            {
                context.Ability.CancelAbility();
            }
            Complete();
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return default;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}