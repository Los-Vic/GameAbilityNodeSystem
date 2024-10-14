using NodeSystem;
using UnityEngine;

namespace GameAbilitySystem.Logic.Ability
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/AbilityAsset", fileName = "NewAbility")]
    public class AbilityAsset : NodeSystemGraphAsset
    {
        public string abilityName;
        public uint abilityID;
    }
}