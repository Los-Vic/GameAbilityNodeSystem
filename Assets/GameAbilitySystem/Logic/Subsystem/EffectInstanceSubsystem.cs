using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;

namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameEffect> _needTickEffects = new();
        private readonly List<GameEffect> _traverseEffectCache = new();
        private readonly List<GameEffect> _pendingDestroyEffects = new();
        internal HandlerResourceMgr<GameEffect> EffectResourceMgr { get; private set; }

        public override void Init()
        {
            base.Init();
            EffectResourceMgr = new(1024);
        }

        public override void Update(float deltaTime)
        {
            if(_needTickEffects.Count == 0 && _pendingDestroyEffects.Count == 0)
                return;

            _traverseEffectCache.Clear();
            _traverseEffectCache.AddRange(_needTickEffects);

            foreach (var a in _traverseEffectCache)
            {
                a.OnTick();
            }

            for (var i = _pendingDestroyEffects.Count - 1; i >= 0; i--)
            {
                var effect = _pendingDestroyEffects[i];
                if (EffectResourceMgr.GetRefCount(effect.Handler) != 0)
                    continue;
                DisposeEffect(effect);
                _pendingDestroyEffects.RemoveAt(i);
            }
        }

        #region Effect Instance Create/Destroy

        internal GameEffect CreateEffect(ref GameEffectCreateParam param)
        {
            var effect = System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Get<GameEffect>();
            var h = EffectResourceMgr.Create(effect);
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
            if (effect.Handler == 0 || _pendingDestroyEffects.Contains(effect))
                return;
            
            if (EffectResourceMgr.GetRefCount(effect.Handler) == 0)
            {
                DisposeEffect(effect);
            }
            else
            {
                _pendingDestroyEffects.Add(effect);
            }
        }

        private void DisposeEffect(GameEffect effect)
        {
            if (!System.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(effect.Owner, out var owner))
                return;
            GameLogger.Log($"Release Effect: {effect} of {owner}");
            owner.GameEffects.Remove(effect);
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