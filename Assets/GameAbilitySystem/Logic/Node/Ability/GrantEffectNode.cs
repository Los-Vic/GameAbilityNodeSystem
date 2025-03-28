using GameplayCommonLibrary;
using MissQ;
using NS;

namespace GAS.Logic
{
    [Node("GrantEffectForUnit", "Ability/Action/GrantEffectForUnit", ENodeFunctionType.Action, typeof(GrantEffectNodeRunner),
        NodeCategoryDefine.EffectNode, NodeScopeDefine.Ability)]
    public sealed class GrantEffectForUnitNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InFlowPort;

        [Port(EPortDirection.Input, typeof(GameUnit), "Unit")]
        public string InUnitPort;

        [Port(EPortDirection.Input, typeof(FP), "SignalVal1")]
        public string SignalVal1;
        [Port(EPortDirection.Input, typeof(FP), "SignalVal2")]
        public string SignalVal2;
        [Port(EPortDirection.Input, typeof(FP), "SignalVal3")]
        public string SignalVal3;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutFlowPort;
    }
    
    public sealed class GrantEffectNodeRunner : FlowNodeRunner
    {
        private GrantEffectForUnitNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (GrantEffectForUnitNode)nodeAsset;
        }

        public override void Execute()
        {
            var unit = GraphRunner.GetInPortVal<GameUnit>(_node.InUnitPort);
            if (unit == null)
            {
                GameLogger.LogWarning("grant effect failed, unit is null.");
                return;
            }
            
            
        }

        public override void OnReturnToPool()
        {
            _node = null;
            base.OnReturnToPool();
        }
    }
}