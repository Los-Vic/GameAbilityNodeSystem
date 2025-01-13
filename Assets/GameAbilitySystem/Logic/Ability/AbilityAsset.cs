using System;
using System.Collections.Generic;
using GAS.Logic.Target;
using GAS.Logic.Value;
using NS;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    public enum EAbilityTag
    {
        Aura
    }

    public enum EAbilityLifeType
    {
        Activated,  
        ActivatedNums,
        GameEventTriggeredNums,
        Duration,
        KillByGraphLogic
    }

    public enum EAbilityCastStyle
    {
        Simplified,
        Complete
    }
    
    
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
        
        [BoxGroup("Visual")]
        [FilePath] public string cueAssetPath;
        
        [BoxGroup("Life")]
        [DetailedInfoBox("click to show life type descriptions", "Activated: 技能生效一次后死亡\n" +
                                                       "ActivatedNums:技能生效多次后死亡\n" +
                                                       "GameEventTriggeredNums:游戏事件触发一定次数后死亡\n" +
                                                       "Duration:给定生命时长(seconds)\n" +
                                                       "KillByGraphLogic:图里主动调用KillAbility")]
        [OnValueChanged(nameof(OnLifeTypeChanged))]
        public EAbilityLifeType lifeType;

        [BoxGroup("Life"), ShowIf("@lifeType == EAbilityLifeType.ActivatedNums " +
                                  "|| lifeType == EAbilityLifeType.GameEventTriggeredNums " +
                                  "|| lifeType == EAbilityLifeType.Duration")]
        [SerializeReference]
        public ValuePickerBase lifeParamVal;
        
        [BoxGroup("Resource")]
        [SerializeReference]
        public ValuePickerBase cooldown;
        
        [BoxGroup("Resource")]
        public List<AbilityCostCfgElement> costs = new();

        [BoxGroup("Cast")] 
        [OnValueChanged(nameof(OnCastStyleChanged))]
        public EAbilityCastStyle castStyle;

        [BoxGroup("Cast/Time", VisibleIf = "@castStyle == EAbilityCastStyle.Complete")]
        [SerializeReference]
        public ValuePickerBase preCastTime;
        
        [BoxGroup("Cast/Time")]
        [SerializeReference]
        public ValuePickerBase castTime;
        
        [BoxGroup("Cast/Time")]
        [SerializeReference]
        public ValuePickerBase postCastTime;
        
        [BoxGroup("Cast/Time")]
        [SerializeReference]
        [InfoBox("限制完整的cast流程时长。当配置时长大于该值时，会将各部分等比缩小")]
        public ValuePickerBase castProcessTimeClamp;
        
        public List<EAbilityTag> abilityTags = new();
        
        //Runtime
        public uint Id { get; set; }
        
#if UNITY_EDITOR
    
        private void OnLifeTypeChanged()
        {
            if (lifeType is EAbilityLifeType.Activated or EAbilityLifeType.KillByGraphLogic)
            {
                lifeParamVal = null;
            }
        }

        private void OnCastStyleChanged()
        {
            if (castStyle == EAbilityCastStyle.Simplified)
            {
                preCastTime = null;
                castTime = null;
                postCastTime = null;
                castProcessTimeClamp = null;
            }
        }
    
#endif
    }
}