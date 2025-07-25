using NS;

namespace GAS.Logic
{
    [Node("StopAbilityFx", "AbilitySystem/Action/StopAbilityFx", ENodeFunctionType.Action, typeof(StopAbilityFxNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class StopAbilityFxNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Exposed] 
        public string CueName;
    }
    
    public sealed class StopAbilityFxNodeRunner : FlowNodeRunner
    {
        private StopAbilityFxNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (StopAbilityFxNode)context.Node;
        }
        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var stopContext = new StopAbilityFxCueContext()
            {
                UnitHandler = context.Ability.Owner,
                AbilityHandler = context.Ability.Handler,
                GameCueName = _node.CueName,
            };
           
            context.Ability.AbilityCue.StopAbilityFxCue(ref stopContext);
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