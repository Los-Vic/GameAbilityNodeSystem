using NS;

namespace GAS.Logic
{
    [Node("StopAbilityFx", "AbilitySystem/Action/StopAbilityFx", ENodeType.Action, typeof(StopAbilityFxNodeRunner), 
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
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (StopAbilityFxNode)node;
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var stopContext = new StopAbilityFxCueContext()
            {
                UnitHandler = context.Ability.Owner,
                AbilityHandler = context.Ability.Handler,
                GameCueName = n.CueName,
            };
           
            context.Ability.AbilityCue.StopAbilityFxCue(ref stopContext);
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n  = (StopAbilityFxNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}