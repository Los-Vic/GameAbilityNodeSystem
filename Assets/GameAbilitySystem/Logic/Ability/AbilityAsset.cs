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
        [SerializeReference]
        public TargetSelectBase costTarget;
        public ESimpleAttributeType attributeType;
        [BoxGroup("CostVal"), HideLabel]
        [SerializeReference]
        public ValuePickerBase costVal;
        
    }
    
    [CreateAssetMenu(menuName = "GameAbilitySystem/AbilityAsset", fileName = "NewAbility")]
    public class AbilityAsset : NodeGraphAsset
    {
        [Title("Ability")]
        public string abilityName;
        
        [InfoBox("表现对象")]
        [FilePath] public string cueProxy;
        
        [InfoBox("短暂——效果激活结束以后自动销毁；持续——主动调用销毁逻辑")]
        public EAbilityLifeType lifeType;
        
        [SerializeReference]
        public ValuePickerBase cooldown;
        
        public List<AbilityCostCfgElement> costs = new();
        
        public List<EAbilityTag> abilityTags = new();
        
        //Runtime
        public uint Id { get; set; }
    }
}