﻿using System.Collections.Generic;

namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly List<GameEffect> _needTickEffects = new();
        private readonly List<GameEffect> _traverseEffectCache = new();


        public override void Update(float deltaTime)
        {
            if(_needTickEffects.Count == 0)
                return;

            _traverseEffectCache.Clear();
            foreach (var a in _needTickEffects)
            {
                _traverseEffectCache.Add(a);
            }

            foreach (var a in _traverseEffectCache)
            {
                a.OnTick();
            }
        }

        #region Effect Instance Create/Destroy

        internal GameEffect CreateEffect(ref GameEffectCreateParam param)
        {
            var effect = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<GameEffect>();
            effect.Init(ref param);
            return effect;
        }
        
        internal void DestroyEffect(GameEffect effect)
        {
            effect.GetRefCountDisposableComponent().MarkForDispose();
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