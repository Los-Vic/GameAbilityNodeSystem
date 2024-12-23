using System;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    public class AbilityInstanceMgr
    {
        private readonly GameAbilitySystem _system;
        public AbilityInstanceMgr(GameAbilitySystem sys)
        {
            _system = sys;
        }

        internal GameAbility CreateAbility(uint id)
        {
            var abilityAsset = _system.AssetConfigProvider.GetAbilityAsset(id);
            if (abilityAsset == null)
            {
                NodeSystemLogger.LogError($"Fail to get ActiveAbilityAsset of {id}");
                return default;
            }
            
            var ability = _system.ObjectPoolMgr.CreateObject<GameAbility>();
            var param = new AbilityCreateParam()
            {
                Asset = abilityAsset
            };
            
            ability.Init(_system, ref param);
            NodeSystemLogger.Log($"Created Ability: {param.Asset.abilityName}");
            return ability;
        }

        internal void DestroyAbility(GameAbility ability)
        {
            NodeSystemLogger.Log($"Destroy Ability: {ability.Asset.abilityName}");
            _system.ObjectPoolMgr.DestroyObject(ability);
        }
    }
}