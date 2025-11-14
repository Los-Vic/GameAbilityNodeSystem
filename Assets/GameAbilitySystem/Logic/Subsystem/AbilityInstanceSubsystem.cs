using System.Collections.Generic;
using Gameplay.Common;

namespace GAS.Logic
{
    //Ability的Handler不应该AddRef
    public class AbilityInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameAbility> _needTickAbilities = new();
        private readonly List<GameAbility> _traverseAbilityCache = new();

        public override void Init()
        {
            base.Init();
            System.HandlerManagers.AbilityHandlerMgr.Init(GetAbility, DisposeAbility, 1024);
        }

        public override void UnInit()
        {
            _needTickAbilities.Clear();
            _traverseAbilityCache.Clear();
            System.HandlerManagers.AbilityHandlerMgr.UnInit();
            base.UnInit();
        }

        public override void Update(float deltaTime)
        {
            if(_needTickAbilities.Count == 0)
                return;

            _traverseAbilityCache.Clear();
            _traverseAbilityCache.AddRange(_needTickAbilities);
            foreach (var a in _traverseAbilityCache)
            {
                a.OnTick();
            }
        }

        internal GameAbility CreateAbility(ref AbilityCreateParam param)
        {
            var abilityAsset = System.AssetConfigProvider.GetAbilityAsset(param.Id);
            if (!abilityAsset)
            {
                GameLogger.LogError($"Fail to get ActiveAbilityAsset:{param.Id}");
                return null;
            }
            
            var h = System.HandlerManagers.AbilityHandlerMgr.Create();
            System.HandlerManagers.AbilityHandlerMgr.DeRef(h, out var ability);

            var initParam = new AbilityInitParam()
            {
                CreateParam = param,
                Handler = h
            };
            ability.Init(abilityAsset, ref initParam);
            return ability;
        }
        internal void DestroyAbility(GameAbility ability)
        {
            if (ability.State is EAbilityState.MarkDestroy or EAbilityState.UnInitialized)
                return;
            
            ability.MarkDestroy();
            RemoveFromTickList(ability);
            System.HandlerManagers.AbilityHandlerMgr.RemoveRefCount(ability.Handler);
        }

        private GameAbility GetAbility()
        {
            return System.ClassObjectPoolSubsystem.Get<GameAbility>();
        }
        
        private void DisposeAbility(Handler<GameAbility> h, GameAbility ability)
        {
            GameLogger.Log($"Release ability:{ability}");
            System.ClassObjectPoolSubsystem.Release(ability);
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