using System.Collections.Generic;
using NS;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    [System.Serializable]
    public class AbilityEffectParam
    {
        public string paramName;
        public List<float> paramVals = new();
    }
    
    [CreateAssetMenu(fileName = "AbilityEffectParamConfig", menuName = "GameAbilitySystem/AbilityEffectParamConfig")]
    public class AbilityEffectParamConfig:ScriptableObject, IEnumStringProvider
    {
        [Searchable]
        public List<AbilityEffectParam> paramElements = new();

        public List<string> GetEnumStringList()
        {
            var stringList = new List<string>();
            foreach (var param in paramElements)
            {
                stringList.Add(param.paramName);
            }
            return stringList;
        }
    }
}