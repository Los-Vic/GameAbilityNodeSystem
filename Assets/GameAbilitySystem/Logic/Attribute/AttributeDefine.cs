using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [DrawWithUnity]
    public enum ESimpleAttributeType
    {
        //通用
        None = 0,
        [InspectorName("Common/Level")]
        Level = 1,
        [InspectorName("Common/ID")]
        ID = 2,
        [InspectorName("Common/PlayerID")]
        PlayerID = 3,
        [InspectorName("Common/PlayerCampID")]
        PlayerCampID = 4,
        
        //单位
        [InspectorName("Unit/Mana")]
        Mana = 100,
        [InspectorName("Unit/AttackBase")]
        AttackBase = 101,
        [InspectorName("Unit/AttackAdd")]
        AttackAdd = 102,
        
    }
    
    [DrawWithUnity]
    public enum ECompositeAttributeType
    {
        None = 0,
        Attack = 1, 
    }
}