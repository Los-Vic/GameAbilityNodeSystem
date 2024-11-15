using System;
using UnityEngine;

namespace GameAbilitySystem.Logic
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
            var abilityAsset = _system.AbilityAssetProvider.GetAbilityAsset(id);
            if (abilityAsset == null)
            {
                Debug.LogError($"Fail to get ActiveAbilityBp of {id}");
                return default;
            }
            
            var ability = _system.ObjectPoolMgr.CreateObject<GameAbility>();
            var param = new AbilityCreateParam()
            {
                Asset = abilityAsset
            };
            
            ability.Init(ref param);
            return ability;
        }

        internal void DestroyAbility(GameAbility ability)
        {
            _system.ObjectPoolMgr.DestroyObject(ability);
        }
    }
}