using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("PlayAbilityFx", "AbilitySystem/Action/PlayAbilityFx", ENodeFunctionType.Action, typeof(PlayAbilityFxNodeRunner), 
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
    }
    
    public sealed class PlayAbilityFxNodeRunner : FlowNodeRunner
    {
        private PlayAbilityFxNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (PlayAbilityFxNode)nodeAsset;
        }

        public override void Execute()
        {
            base.Execute();
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var playContext = new PlayAbilityFxCueContext
            {
                UnitHandler = context.Ability.Owner,
                AbilityHandler = context.Ability.Handler,
                GameCueName = _node.CueName,
                Param = GraphRunner.GetInPortVal<FP>(_node.InParam)
            };
            var target = GraphRunner.GetInPortVal<GameUnit>(_node.InTargetUnit);
            if (target != null)
            {
                playContext.SubUnitHandler = target.Handler;
            }
           
            context.Ability.Sys.GameCueSubsystem.PlayAbilityFxCue(ref playContext);
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