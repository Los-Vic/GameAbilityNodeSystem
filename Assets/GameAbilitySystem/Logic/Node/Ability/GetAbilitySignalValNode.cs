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
    
    [Node("GetAbilitySignalValNode", "AbilitySystem/Value/GetAbilitySignalValNode", ENodeType.Value , typeof(GetAbilitySignalValNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class GetAbilitySignalValNode:Node
    {
        [Exposed]
        public EAbilitySignalVal SignalVal;
        
        [Port(EPortDirection.Output, typeof(FP),"Val")]
        public string OutPortVal;
    }

    public sealed class GetAbilitySignalValNodeRunner : NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var n = (GetAbilitySignalValNode)node;
            var val = n.SignalVal switch
            {
                EAbilitySignalVal.SignalVal1 => context.Ability.SignalVal1,
                EAbilitySignalVal.SignalVal2 => context.Ability.SignalVal2,
                EAbilitySignalVal.SignalVal3 => context.Ability.SignalVal3,
                _ => 0
            };
            graphRunner.SetOutPortVal(n.OutPortVal, val);
        }
    }
}