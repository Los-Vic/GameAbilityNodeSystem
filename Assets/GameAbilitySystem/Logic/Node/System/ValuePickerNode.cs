using GAS.Logic.Value;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("ValuePicker", "AbilitySystem/Value/ValuePicker", ENodeFunctionType.Value , typeof(ValuePickerNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class ValuePickerNode:Node
    {
        [Exposed]
        [SerializeReference]
        public ValuePickerBase Config;

        [Port(EPortDirection.Input, typeof(GameUnit), "Unit")]
        public string InPortUnit;
        
        [Port(EPortDirection.Input, typeof(FP), "Lv(opt)")]
        public string InPortLv;
        
        [Port(EPortDirection.Output, typeof(FP),"Val")]
        public string OutPortVal;
    }

    public sealed class ValuePickerNodeRunner : NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (ValuePickerNode)node;
            var lv = graphRunner.GetInPortVal<FP>(n.InPortLv);
            var unit = graphRunner.GetInPortVal<GameUnit>(n.InPortUnit);
            graphRunner.SetOutPortVal(n.OutPortVal, ValuePickerUtility.GetValue(n.Config, unit, (uint)lv));
        }
    }
}