using System.Collections.Generic;
using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class AbilityInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameAbility> _needTickAbilities = new();
        private readonly List<GameAbility> _traverseAbilityCache = new();
        
        public override void Update(float deltaTime)
        {
            if(_needTickAbilities.Count == 0)
                return;

            _traverseAbilityCache.Clear();
            foreach (var a in _needTickAbilities)
            {
                _traverseAbilityCache.Add(a);
            }

            foreach (var a in _traverseAbilityCache)
            {
                a.OnTick(deltaTime);
            }
        }

        internal GameAbility CreateAbility(uint id)
        {
            var abilityAsset = System.AssetConfigProvider.GetAbilityAsset(id);
            if (abilityAsset == null)
            {
                GameLogger.LogError($"Fail to get ActiveAbilityAsset of {id}");
                return null;
            }
            
            var ability = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<GameAbility>();
            var param = new AbilityCreateParam()
            {
                Asset = abilityAsset
            };
            
            ability.Init(System, ref param);
            GameLogger.Log($"Created Ability: {param.Asset.abilityName}");
            return ability;
        }
        internal void DestroyAbility(GameAbility ability)
        {
            GameLogger.Log($"Destroy Ability: {ability.Asset.abilityName}");
            RemoveFromTickList(ability);
            ability.GetRefCountDisposableComponent().MarkForDispose();
        }

        internal void AddToTickList(GameAbility ability)
        {
            if(_needTickAbilities.Contains(ability))
                return;
            
            _needTickAbilities.Add(ability);
        }

        internal void RemoveFromTickList(GameAbility ability)
        {
            _needTickAbilities.Remove(ability);
        }
    }
}