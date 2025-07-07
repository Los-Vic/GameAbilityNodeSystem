using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;

namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameEffect> _needTickEffects = new();
        private readonly List<GameEffect> _traverseEffectCache = new();
        internal HandlerRscMgr<GameEffect> EffectRscMgr { get; private set; }

        public override void Init()
        {
            base.Init();
            EffectRscMgr = new(1024, DisposeEffect);
        }

        public override void Update(float deltaTime)
        {
            if(_needTickEffects.Count == 0)
                return;

            _traverseEffectCache.Clear();
            _traverseEffectCache.AddRange(_needTickEffects);

            foreach (var a in _traverseEffectCache)
            {
                a.OnTick();
            }
        }

        #region Effect Instance Create/Destroy

        internal GameEffect CreateEffect(ref GameEffectCreateParam param)
        {
            var effect = System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Get<GameEffect>();
            var h = EffectRscMgr.CreateHandler(effect);
            var initParam = new GameEffectInitParam()
            {
                CreateParam = param,
                Handler = h
            };
            effect.Init(System, ref initParam);
            return effect;
        }
        
        internal void DestroyEffect(GameEffect effect)
        {
            if (effect.Handler == 0 || effect.MarkDestroy)
                return;

            effect.MarkDestroy = true;
            EffectRscMgr.RemoveRefCount(effect.Handler);
        }

        private void DisposeEffect(GameEffect effect)
        {
            if (System.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(effect.Owner, out var owner))
            {
                GameLogger.Log($"Release Effect: {effect} of {owner}");
                owner.GameEffects.Remove(effect);
            }
            System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Release(effect);
        }

        #endregion
        
        internal void AddToTickList(GameEffect effect)
        {
            if(_needTickEffects.Contains(effect))
                return;
            
            _needTickEffects.Add(effect);
        }

        internal void RemoveFromTickList(GameEffect effect)
        {
            _needTickEffects.Remove(effect);
        }
    }
}