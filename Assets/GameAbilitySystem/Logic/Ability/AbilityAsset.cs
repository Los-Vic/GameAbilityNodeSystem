using System;
using System.Collections.Generic;
using GAS.Logic.Value;
using NS;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [Serializable]
    public struct CostElement
    {
        [LabelText("属性")]
        public ESimpleAttributeType attributeType;
        [BoxGroup("消耗值"), HideLabel]
        [SerializeReference]
        public ValuePickerBase costVal;
        
    }
    
    [CreateAssetMenu(menuName = "GameAbilitySystem/AbilityAsset", fileName = "NewAbility")]
    public class AbilityAsset : NodeSystemGraphAsset
    {
        [Title("技能")]
        [LabelText("技能名")]
        public string abilityName;

        [BoxGroup("冷却"), HideLabel]
        [SerializeReference]
        public ValuePickerBase cooldown;

        [BoxGroup("消耗")]
        [LabelText("消耗属性列表")]
        public List<CostElement> costs = new();
        
        [BoxGroup("更新")]
        [LabelText("每帧更新")]
        [InfoBox("不需要冷却和延迟等基于时间的行为，则不需要勾选")]
        public bool isTickable = true;
    }
}