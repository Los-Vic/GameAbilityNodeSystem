using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GAS.Logic.Value
{
    [Serializable]
    public class ValuePickerParam:ValuePickerBase
    {
        [ValueDropdown(nameof(GetAllParams))]
        [LabelText("参数名")]
        public string paramName;
        
#if UNITY_EDITOR
        public static List<string> GetAllParams()
        {
            var result = new List<string>();
            var cfg = AssetDatabase.LoadAssetAtPath<AbilityEffectParamConfig>(
                "Assets/GameAbilitySystem/Assets/Configs/AbilityEffectParamConfig.asset");
            if (cfg == null)
                return result;
            
            foreach (var element in cfg.paramElements)
            {
                result.Add(element.paramName);
            }
            return result;
        }
#endif
        
    }
    
    
}