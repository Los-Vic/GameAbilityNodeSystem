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
        Aura,
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
        
        [BoxGroup("Resource")]
        [SerializeReference]
        public ValuePickerBase cooldown;
        
        [BoxGroup("Resource")]
        public List<AbilityCostCfgElement> costs = new();

        [BoxGroup("AbilityTag")]
        public List<EAbilityTag> abilityTags = new();
        
        //Runtime
        public uint Id { get; set; }
        
#if UNITY_EDITOR
        
        /// <summary>
        /// 收集引用的Assets，用于加载的时候分析引用关系
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