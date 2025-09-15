using System.Collections.Generic;
using GCL;
using UnityEngine;

namespace GAS.Logic
{
    public struct PersistentEffectCueContext
    {
        public string CueName;
        public Handler<GameUnit> UnitHandler;
    }
    
    public class EffectGameCue
    {
        private GameAbilitySystem _system;
        private Handler<GameEffect> _handler;
        private readonly Dictionary<int, PersistentEffectCueContext> _persistentContexts = new();

        internal void Init(GameAbilitySystem system, Handler<GameEffect> handler)
        {
            _system = system;
            _handler = handler;
            _persistentContexts.Clear();
        }

        internal void PlayEffectCue(ref PlayEffectFxCueContext context)
        {
            if (context.IsPersistent)
            {
                var hash = Animator.StringToHash(context.GameCueName);
                if (_persistentContexts.ContainsKey(hash))
                {
                    GameLogger.LogWarning($"Effect fx ue already played, ignoring. {context.GameCueName}");
                    return;
                }
                
                _persistentContexts.Add(hash, new PersistentEffectCueContext()
                {
                    CueName = context.GameCueName,
                    UnitHandler = context.UnitHandler,
                });
            }
            
            _system.GameCueSubsystem.PlayEffectCue(ref context);
        }

        internal void StopEffectCue(ref StopEffectFxCueContext context)
        {
            var hash = Animator.StringToHash(context.GameCueName);
            _persistentContexts.Remove(hash);
            
            _system.GameCueSubsystem.StopEffectCue(ref context);
        }
        
        public void ReplayPersistentFx()
        {
            foreach (var pair in _persistentContexts)
            {
                var context = new PlayEffectFxCueContext()
                {
                    GameCueName = pair.Value.CueName,
                    UnitHandler = pair.Value.UnitHandler,
                    EffectHandler = _handler,
                    IsPersistent = true
                };
                _system.GameCueSubsystem.PlayEffectCue(ref context);
            }
        }
    }
}