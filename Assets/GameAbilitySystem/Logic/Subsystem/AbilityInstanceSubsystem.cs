using System.Collections.Generic;
using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class AbilityInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameAbility> _needTickAbilities = new();
        private readonly List<GameAbility> _traverseAbilityCache = new();
        private readonly Dictionary<int, GameAbility> _abilityInstanceLookUp = new();

        public override void UnInit()
        {
            _abilityInstanceLookUp.Clear();
            base.UnInit();
        }

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
                a.OnTick();
            }
        }

        internal GameAbility CreateAbility(ref AbilityCreateParam param)
        {
            var abilityAsset = System.AssetConfigProvider.GetAbilityAsset(param.Id);
            if (abilityAsset == null)
            {
                GameLogger.LogError($"Fail to get ActiveAbilityAsset of {param.Id}");
                return null;
            }
            
            var ability = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<GameAbility>();
            ability.Init(System, abilityAsset, ref param);
            _abilityInstanceLookUp.Add(ability.InstanceID, ability);
            return ability;
        }
        internal void DestroyAbility(GameAbility ability)
        {
            _abilityInstanceLookUp.Remove(ability.InstanceID);
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

        internal GameAbility GetAbilityByInstanceID(int instanceID) =>
            _abilityInstanceLookUp.GetValueOrDefault(instanceID);
    }
}