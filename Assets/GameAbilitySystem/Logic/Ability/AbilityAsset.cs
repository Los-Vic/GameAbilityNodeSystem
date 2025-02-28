using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
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

        [BoxGroup("AbilityTag")]
        public List<EAbilityTag> abilityTags = new();
        
        [Title("References")] 
        [ReadOnly]
        public List<string> referencedAbilities = new();
        [ReadOnly] 
        public List<string> referencedGameCueTags = new();
        
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

        /// <summary>
        /// 收集引用的Ability和GameCue，用于加载的时候分析引用关系
        /// </summary>
        public override void OnGraphNodeChanged()
        {
            Debug.Log($"[Editor]OnGraphNodeChanged graph[{name}], ability[{abilityName}]");
            CollectReferences();
        }

        public override void OnGraphReDraw()
        {
            Debug.Log($"[Editor]OnGraphReDraw graph[{name}], ability[{abilityName}]");
            CollectReferences();
        }

        private void CollectReferences()
        {
            
        }
#endif
    }
}