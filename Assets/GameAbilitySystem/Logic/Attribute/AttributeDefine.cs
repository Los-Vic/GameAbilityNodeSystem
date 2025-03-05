using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [DrawWithUnity]
    public enum ESimpleAttributeType
    {
        //通用
        [InspectorName("无")]
        None = 0,
        [InspectorName("等级")]
        Level = 1,
        [InspectorName("ID")]
        ID = 2,
        
        //随从
        [InspectorName("随从/魔力")]
        MinionMana = 100,
        
    }

    [DrawWithUnity]
    public enum ECompositeAttributeType
    {
        [InspectorName("无")]
        None = 0,
    }
}