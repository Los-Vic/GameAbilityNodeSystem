using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [System.Serializable]
    public class AbilityConfigElement
    {
        [TableColumnWidth(100, Resizable = false)]
        public uint id;
        [FilePath]
        public string abilityAssetPath;
    }
    
    [CreateAssetMenu(fileName = "AbilityConfig", menuName = "GameAbilitySystem/AbilityConfig")]
    public class AbilityConfig:ScriptableObject
    {
        [Searchable]
        [TableList(ShowIndexLabels = true)]
        public List<AbilityConfigElement> elements = new();
    }
}