using System;
using System.Collections.Generic;
using GAS.Logic.Target;
using GAS.Logic.Value;
using NS;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [Serializable]
    public struct AbilityCostCfgElement
    {
        [LabelText("选择目标")] 
        [SerializeReference]
        public TargetSelectBase target;
        [LabelText("属性")]
        public ESimpleAttributeType attributeType;
        [BoxGroup("消耗值"), HideLabel]
        [SerializeReference]
        public ValuePickerBase costVal;
        
    }
    
    [CreateAssetMenu(menuName = "GameAbilitySystem/AbilityAsset", fileName = "NewAbility")]
    public class AbilityAsset : NodeGraphAsset
    {
        [Title("技能")]
        [LabelText("技能名")]
        public string abilityName;

        [LabelText("表现对象")]
        [FilePath] public string cueProxy;
        
        [LabelText("生命")]
        [InfoBox("短暂——效果激活结束以后自动销毁；持续——主动调用销毁逻辑")]
        public EAbilityLifeType lifeType;
        
        [SerializeReference]
        [LabelText("冷却")]
        public ValuePickerBase cooldown;
        
        [LabelText("消耗")]
        public List<AbilityCostCfgElement> costs = new();
        
        [LabelText("技能标签")]
        public List<EAbilityTag> tags = new();
        
        //Runtime
        public uint Id { get; set; }
    }
}