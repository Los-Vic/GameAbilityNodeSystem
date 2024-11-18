using UnityEngine;

namespace GAS.Logic
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/ConfigHub", fileName = "ConfigHub")]
    public class ConfigHub:ScriptableObject
    {
        public AbilityConfig abilityConfig;
        public AbilityEffectParamConfig abilityEffectParamConfig;
    }
}