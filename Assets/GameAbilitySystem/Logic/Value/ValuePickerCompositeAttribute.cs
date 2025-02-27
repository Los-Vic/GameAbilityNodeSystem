using System;
using UnityEngine;

namespace GAS.Logic.Value
{
    [Serializable]
    public class ValuePickerCompositeAttribute:ValuePickerBase
    {
        [Header("二级属性")]
        public ECompositeAttributeType compositeAttributeType;
    }
}