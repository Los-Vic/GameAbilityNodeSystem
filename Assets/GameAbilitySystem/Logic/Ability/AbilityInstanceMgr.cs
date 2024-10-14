using System;
using UnityEngine;

namespace GameAbilitySystem.Logic.Ability
{
    public class AbilityInstanceMgr<T> where T:IEquatable<T>, IComparable<T>
    {
        private readonly GameAbilitySystem<T> _system;

        public AbilityInstanceMgr(GameAbilitySystem<T> sys)
        {
            _system = sys;
        }

        internal GameAbility<T> CreateAbility(uint id)
        {
            var abilityAsset = _system.AbilityAssetProvider.GetAbilityAsset(id);
            if (abilityAsset == null)
            {
                Debug.LogError($"Fail to get ActiveAbilityBp of {id}");
                return default;
            }
            
            var ability = _system.ObjectPoolMgr.CreateObject<GameAbility<T>>();
            var param = new AbilityCreateParam()
            {
                Asset = abilityAsset
            };
            
            ability.Init(ref param);
            return ability;
        }

        internal void DestroyAbility(GameAbility<T> ability)
        {
            _system.ObjectPoolMgr.DestroyObject(ability);
        }
    }
}