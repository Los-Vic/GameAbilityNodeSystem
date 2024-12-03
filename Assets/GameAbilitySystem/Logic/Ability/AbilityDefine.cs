using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [DrawWithUnity]
    public enum ESimpleAttributeType
    {
        [InspectorName("无")]
        None = 0,
        [InspectorName("随从/魔力")]
        MinionMana = 1
    }

    public enum ECompositeAttributeType
    {
        //这里添加新的属性
    }
}