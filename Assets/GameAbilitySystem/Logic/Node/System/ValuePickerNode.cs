using GAS.Logic.Value;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("ValuePicker", "System/Value/ValuePicker", ENodeFunctionType.Value , typeof(ValuePickerNodeRunner), CommonNodeCategory.Value)]
    public class ValuePickerNode:Node
    {
        [ExposedProp]
        [SerializeReference]
        public ValuePickerBase Config;

        [Port(EPortDirection.Input, typeof(GameUnit), "Unit")]
        public string InPortUnit;
        
        [Port(EPortDirection.Output, typeof(FP),"Val")]
        public string OutPortVal;
    }

    public class ValuePickerNodeRunner : NodeRunner
    {
        
    }
}