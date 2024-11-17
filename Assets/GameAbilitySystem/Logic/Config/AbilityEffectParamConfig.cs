using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameAbilitySystem.Logic
{
    [System.Serializable]
    public class AbilityEffectParam
    {
        public string paramName;
        public List<float> paramVals = new();
    }
    
    [CreateAssetMenu(fileName = "AbilityEffectParamConfig", menuName = "GameAbilitySystem/AbilityEffectParamConfig")]
    public class AbilityEffectParamConfig:ScriptableObject
    {
        [Searchable]
        public List<AbilityEffectParam> paramElements = new();
    }
}