using System;
using System.Collections.Generic;
using NS;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GAS.Logic
{
    [Serializable]
    public struct CostElement
    {
        [LabelText("属性")]
        public ESimpleAttributeType attributeType;
        [LabelText("消耗值")]
        [ValueDropdown(nameof(GetParams))]
        public string costVal;
        
#if UNITY_EDITOR
        public static List<string> GetParams => AbilityAsset.GetParams();
#endif
    }
    
    [CreateAssetMenu(menuName = "GameAbilitySystem/AbilityAsset", fileName = "NewAbility")]
    public class AbilityAsset : NodeSystemGraphAsset
    {
        [Title("技能")]
        [LabelText("技能名")]
        public string abilityName;

        [LabelText("冷却")]
        [ValueDropdown(nameof(GetParams))]
        public string cooldown;

        [LabelText("消耗")]
        public List<CostElement> costs = new();
        
        [LabelText("每帧更新")]
        public bool isTickable = true;
        
        #if UNITY_EDITOR
        public static List<string> GetParams()
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