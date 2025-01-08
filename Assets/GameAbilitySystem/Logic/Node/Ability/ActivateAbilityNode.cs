using NS;

namespace GAS.Logic
{
    [Node("ActivateAbility", "Ability/Exec/ActivateAbility", ENodeFunctionType.Flow, typeof(ActivateAbilityNodeRunner), CommonNodeCategory.Action, NodeScopeDefine.Ability)]
    public class ActivateAbilityNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
    }

    public class ActivateAbilityNodeRunner : FlowNodeRunner
    {
        private ActivateAbilityNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ActivateAbilityNode)nodeAsset;
        }

        public override void Execute()
        {
            if (GraphRunner.Context is GameAbilityGraphRunnerContext context)
            {
                context.Ability.GF_ActivateAbility();
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