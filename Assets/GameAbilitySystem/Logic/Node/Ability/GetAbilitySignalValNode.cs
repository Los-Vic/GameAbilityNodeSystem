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
    
    [Node("GetAbilitySignalValNode", "AbilitySystem/Value/GetAbilitySignalValNode", ENodeFunctionType.Value , typeof(GetAbilitySignalValNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class GetAbilitySignalValNode:Node
    {
        [Exposed]
        public EAbilitySignalVal SignalVal;
        
        [Port(EPortDirection.Output, typeof(FP),"Val")]
        public string OutPortVal;
    }

    public sealed class GetAbilitySignalValNodeRunner : NodeRunner
    {
        private GetAbilitySignalValNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (GetAbilitySignalValNode)context.Node;
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