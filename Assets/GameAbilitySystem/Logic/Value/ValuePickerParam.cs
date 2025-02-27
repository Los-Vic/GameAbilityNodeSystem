using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GAS.Logic.Value
{
    [Serializable]
    public class ValuePickerParam:ValuePickerBase
    {
        [Header("参数")]
        [ValueDropdown(nameof(GetAllParams))]
        public string paramName;
        
        public static List<string> GetAllParams()
        {
            var result = new List<string>();
#if UNITY_EDITOR
            var cfg = AssetDatabase.LoadAssetAtPath<AbilityEffectParamConfig>(
                "Assets/GameAbilitySystem/Assets/Configs/AbilityEffectParamConfig.asset");
            if (cfg == null)
                return result;
            
            foreach (var element in cfg.paramElements)
            {
                result.Add(element.paramName);
            }
#endif
            return result;
        }

        
    }
    
    
}