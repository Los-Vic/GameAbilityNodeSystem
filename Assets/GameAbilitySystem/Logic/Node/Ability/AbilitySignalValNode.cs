using MissQ;
using NS;

namespace GAS.Logic
{
    public enum EAbilitySignalVal
    {
        SignalVal1,
        SignalVal2,
        SignalVal3,
    }
    
    [Node("AbilitySignalVal", "AbilitySystem/Value/AbilitySignalVal", ENodeFunctionType.Value , typeof(AbilitySignalValNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class AbilitySignalValNode:Node
    {
        [Exposed]
        public EAbilitySignalVal SignalVal;
        
        [Port(EPortDirection.Output, typeof(FP),"Val")]
        public string OutPortVal;
    }

    public sealed class AbilitySignalValNodeRunner : NodeRunner
    {
        private AbilitySignalValNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AbilitySignalValNode)nodeAsset;
        }

        public override void Execute()
        {
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var val = _node.SignalVal switch
            {
                EAbilitySignalVal.SignalVal1 => context.Ability.SignalVal1,
                EAbilitySignalVal.SignalVal2 => context.Ability.SignalVal2,
                EAbilitySignalVal.SignalVal3 => context.Ability.SignalVal3,
                _ => 0
            };
            GraphRunner.SetOutPortVal(_node.OutPortVal, val);
        }

        public override void OnReturnToPool()
        {
            _node = null;
            base.OnReturnToPool();
        }
    }
}