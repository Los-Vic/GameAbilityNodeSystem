using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("PlayAbilityFx", "AbilitySystem/Action/PlayAbilityFx", ENodeType.Action, typeof(PlayAbilityFxNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class PlayAbilityFxNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(EPortDirection.Input, typeof(GameUnit), "Target(Op)")]
        public string InTargetUnit;
        [Port(EPortDirection.Input, typeof(FP), "Param(Op)")]
        public string InParam;

        [Exposed] 
        public string CueName;

        [Exposed] 
        public bool IsPersistent;
    }
    
    public sealed class PlayAbilityFxNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n =  (PlayAbilityFxNode)node;
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var playContext = new PlayAbilityFxCueContext
            {
                UnitHandler = context.Ability.Owner,
                AbilityHandler = context.Ability.Handler,
                GameCueName = n.CueName,
                Param = graphRunner.GetInPortVal<FP>(n.InParam),
                IsPersistent = n.IsPersistent
            };
            var target = graphRunner.GetInPortVal<GameUnit>(n.InTargetUnit);
            if (target != null)
            {
                playContext.SubUnitHandler = target.Handler;
            }
           
            context.Ability.AbilityCue.PlayAbilityFxCue(ref playContext);
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (PlayAbilityFxNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}