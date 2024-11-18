using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [System.Serializable]
    public class AbilityConfigElement
    {
        public uint id;
        public string logicAssetPath;
        public string proxyAssetPath;
    }
    
    [CreateAssetMenu(fileName = "AbilityConfig", menuName = "GameAbilitySystem/AbilityConfig")]
    public class AbilityConfig:ScriptableObject
    {
        [Searchable]
        public List<AbilityConfigElement> elements = new();
    }
}