using System.Collections.Generic;
using GCL;

namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameEffect> _needTickEffects = new();
        private readonly List<GameEffect> _traverseEffectCache = new();

        public override void Init()
        {
            base.Init();
            Singleton<HandlerMgr<GameEffect>>.Instance.Init(GetEffect, DisposeEffect, 1024);
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
            var h = Singleton<HandlerMgr<GameEffect>>.Instance.CreateHandler();
            Singleton<HandlerMgr<GameEffect>>.Instance.DeRef(h, out var effect);
            var initParam = new GameEffectInitParam()
            {
                CreateParam = param,
                Handler = h
            };
            effect.Init(ref initParam);
            return effect;
        }
        
        internal void DestroyEffect(GameEffect effect)
        {
            if (effect.Handler == 0 || effect.MarkDestroy)
                return;

            effect.MarkDestroy = true;
            Singleton<HandlerMgr<GameEffect>>.Instance.RemoveRefCount(effect.Handler);
        }

        private GameEffect GetEffect()
        {
            return System.ClassObjectPoolSubsystem.Get<GameEffect>();
        }
        
        private void DisposeEffect(GameEffect effect)
        {
            GameLogger.Log($"Release effect:{effect}");
            System.ClassObjectPoolSubsystem.Release(effect);
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