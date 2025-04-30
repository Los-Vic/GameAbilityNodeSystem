using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GAS.Logic
{
    [System.Serializable]
    public class AbilityConfigElement
    {
        [TableColumnWidth(100, Resizable = false)]
        public uint id;
        [Sirenix.OdinInspector.FilePath]
        public string abilityAssetPath;
    }
    
    [CreateAssetMenu(fileName = "AbilityConfig", menuName = "GameAbilitySystem/AbilityConfig")]
    public class AbilityConfig:ScriptableObject
    {
        [Searchable]
        [TableList(ShowIndexLabels = true)]
        public List<AbilityConfigElement> elements = new();
        
        #if UNITY_EDITOR
        [Button("RefreshAbilityAssetId", ButtonSizes.Large)]
        private void RefreshAbilityAssetId()
        {
            foreach (var element in elements)
            {
                var obj = AssetDatabase.LoadAssetAtPath<AbilityAsset>(element.abilityAssetPath);
                obj.id = element.id;
            }
        }
        
        #endif
    }
}