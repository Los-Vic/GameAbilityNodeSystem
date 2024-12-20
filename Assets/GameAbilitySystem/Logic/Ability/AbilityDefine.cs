using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
   public enum EAbilityTag
   {
      
   }

   public enum EAbilityLifeType
   {
      [InspectorName("短暂")]
      Instant, // 激活一次后自动销毁
      [InspectorName("持续")]
      Persistent //主动调用销毁
   }
}