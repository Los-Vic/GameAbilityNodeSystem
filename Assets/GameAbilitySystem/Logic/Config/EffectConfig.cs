using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [System.Serializable]
    public class EffectConfigElement
    {
        [TableColumnWidth(100, Resizable = false)]
        public uint id;
        [FilePath]
        public string effectAssetPath;
    }
    
    [CreateAssetMenu(fileName = "EffectConfig", menuName = "GameAbilitySystem/EffectConfig")]
    public class EffectConfig:ScriptableObject
    {
        [Searchable]
        [TableList(ShowIndexLabels = true)]
        public List<EffectConfigElement> elements = new();
    }
}