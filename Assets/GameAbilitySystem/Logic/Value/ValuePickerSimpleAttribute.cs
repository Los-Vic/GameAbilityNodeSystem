using System;
using UnityEngine;

namespace GAS.Logic.Value
{
    [Serializable]
    public class ValuePickerSimpleAttribute:ValuePickerBase
    {
        [Header("一级属性")]
        public ESimpleAttributeType simpleAttributeType;
    }
}