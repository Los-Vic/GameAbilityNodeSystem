using NS;

namespace GAS.Logic
{
    [Node("ActivateAbilityByEvent", "Ability/Exec/ActivateAbilityByEvent", ENodeFunctionType.Flow, typeof(ActivateAbilityByEventNodeRunner), CommonNodeCategory.Action, NodeScopeDefine.Ability)]
    public class ActivateAbilityByEventNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(EPortDirection.Input, typeof(GameEventNodeParam), "EventParam")]
        public string InPortVal;
    }

    public class ActivateAbilityByEventNodeRunner : FlowNodeRunner
    {
        private ActivateAbilityByEventNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ActivateAbilityByEventNode)nodeAsset;
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(_node.Id);
            var param = GraphRunner.GetInPortVal<GameEventNodeParam>(_node.InPortVal);
            if (GraphRunner.Context is GameAbilityGraphRunnerContext context)
            {
                context.Ability.GF_ActivateAbilityWithGameEventParam(param);
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