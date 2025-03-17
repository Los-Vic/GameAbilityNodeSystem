using System.Collections.Generic;
using GAS.Logic.Value;
using NS;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/EffectAsset", fileName = "NewEffect")]
    public class EffectAsset : NodeGraphAsset
    {
        [Title("Effect")]
        public string effectName;
        
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
        
#if UNITY_EDITOR

        /// <summary>
        /// 收集引用的Assets，用于加载的时候分析引用关系
        /// </summary>
        public override void OnGraphNodeChanged()
        {
            Debug.Log($"[Editor]OnGraphNodeChanged graph[{name}], effect[{effectName}]");
            CollectReferences();
        }

        public override void OnGraphReDraw()
        {
            Debug.Log($"[Editor]OnGraphReDraw graph[{name}], effect[{effectName}]");
            CollectReferences();
        }

        private void CollectReferences()
        {
            
        }
#endif
    }
}