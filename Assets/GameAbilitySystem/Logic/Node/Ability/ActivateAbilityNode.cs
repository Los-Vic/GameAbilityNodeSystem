using NS;

namespace GAS.Logic
{
    [Node("ActivateAbility", "Ability/Exec/ActivateAbility", ENodeFunctionType.Flow, typeof(ActivateAbilityNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.Ability, "Trigger OnActivateAbility portal immediately, means activity is activated")]
    public sealed class ActivateAbilityNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(EPortDirection.Input, typeof(GameEventArg), "EventArg")]
        public string InPortVal;
    }

    public sealed class ActivateAbilityNodeRunner : FlowNodeRunner
    {
        private ActivateAbilityNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ActivateAbilityNode)nodeAsset;
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(_node.Id);
            var param = GraphRunner.GetInPortVal<GameEventArg>(_node.InPortVal);
            if (GraphRunner.Context is GameAbilityGraphRunnerContext context)
            {
                context.Ability.ActivateAbilityWithGameEventParam(param);
            }
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
    }
}