using System;
using Sirenix.OdinInspector;

namespace GAS.Logic.Value
{
    [Serializable]
    public class ValuePickerSimpleAttribute:ValuePickerBase
    {
        [LabelText("基础属性类型")]
        public ESimpleAttributeType simpleAttributeType;
    }
}