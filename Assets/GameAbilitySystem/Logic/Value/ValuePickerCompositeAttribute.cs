using System;
using Sirenix.OdinInspector;

namespace GAS.Logic.Value
{
    [Serializable]
    public class ValuePickerCompositeAttribute:ValuePickerBase
    {
        [LabelText("复合属性类型")]
        public ECompositeAttributeType compositeAttributeType;
    }
}