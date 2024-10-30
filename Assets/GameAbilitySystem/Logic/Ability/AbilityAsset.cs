using NS;
using UnityEngine;

namespace GameAbilitySystem.Logic.Ability
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/AbilityAsset", fileName = "NewAbility")]
    public class AbilityAsset : NodeSystemGraphAsset
    {
        [Header("Ability Section")]
        public string abilityName;
        public uint abilityID;
    }
}