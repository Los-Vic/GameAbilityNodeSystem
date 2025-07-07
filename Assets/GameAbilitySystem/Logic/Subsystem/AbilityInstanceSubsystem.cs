using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;

namespace GAS.Logic
{
    //Ability的Handler不应该AddRef
    public class AbilityInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameAbility> _needTickAbilities = new();
        private readonly List<GameAbility> _traverseAbilityCache = new();
        private readonly List<GameAbility> _pendingDestroyAbilities = new();
        
        public HandlerResourceMgr<GameAbility> AbilityHandlerRscMgr { get; private set; }

        public override void Init()
        {
            base.Init();
            AbilityHandlerRscMgr = new(1024);
        }

        public override void UnInit()
        {
            _needTickAbilities.Clear();
            _traverseAbilityCache.Clear();
            _pendingDestroyAbilities.Clear();
            base.UnInit();
        }

        public override void Update(float deltaTime)
        {
            if(_needTickAbilities.Count == 0 && _pendingDestroyAbilities.Count == 0)
                return;

            _traverseAbilityCache.Clear();
            _traverseAbilityCache.AddRange(_needTickAbilities);
            foreach (var a in _traverseAbilityCache)
            {
                a.OnTick();
            }

            for (var i = _pendingDestroyAbilities.Count - 1; i >= 0; i--)
            {
                var h = _pendingDestroyAbilities[i].Handler;
                if (AbilityHandlerRscMgr.GetRefCount(h) != 0)
                    continue;
                DisposeAbility(_pendingDestroyAbilities[i]);
                _pendingDestroyAbilities.RemoveAt(i);
            }
        }

        internal GameAbility CreateAbility(ref AbilityCreateParam param)
        {
            var abilityAsset = System.AssetConfigProvider.GetAbilityAsset(param.Id);
            if (!abilityAsset)
            {
                GameLogger.LogError($"Fail to get ActiveAbilityAsset of {param.Id}");
                return null;
            }
            
            var ability = System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Get<GameAbility>();
            var h = AbilityHandlerRscMgr.Create(ability);

            var initParam = new AbilityInitParam()
            {
                CreateParam = param,
                Handler = h
            };
            ability.Init(System, abilityAsset, ref initParam);
            return ability;
        }
        internal void DestroyAbility(GameAbility ability)
        {
            if (ability.State is EAbilityState.MarkDestroy or EAbilityState.UnInitialized)
                return;
            
            ability.MarkDestroy();
            RemoveFromTickList(ability);
            if (AbilityHandlerRscMgr.GetRefCount(ability.Handler) > 0)
            {
                _pendingDestroyAbilities.Add(ability);
            }
            else
            {
                DisposeAbility(ability);
            }
        }

        private void DisposeAbility(GameAbility ability)
        {
            if (System.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(ability.Owner, out var owner))
            {
                GameLogger.Log($"Release Ability: {ability}");
                owner.GameAbilities.Remove(ability);
            }
            
            System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Release(ability);
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

        internal bool GetAbilityByHandler(Handler<GameAbility> h, out GameAbility ability) =>
            AbilityHandlerRscMgr.Dereference(h, out ability);
    }
}