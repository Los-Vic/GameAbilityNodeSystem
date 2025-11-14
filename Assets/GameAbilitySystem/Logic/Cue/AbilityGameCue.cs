using System.Collections.Generic;
using Gameplay.Common;
using MissQ;
using UnityEngine;

namespace GAS.Logic
{
    public struct PersistentAbilityCueContext
    {
        public string CueName;
        public Handler<GameUnit> UnitHandler;
        public Handler<GameUnit> SubUnitHandler;
        public FP Param;
    }
    
    public class AbilityGameCue
    {
        private GameAbilitySystem _system;
        private Handler<GameAbility> _handler;
        private readonly Dictionary<int, PersistentAbilityCueContext> _persistentContexts = new ();

        internal void Init(GameAbilitySystem system, Handler<GameAbility> handler)
        {
            _handler = handler;
            _system = system;
            _persistentContexts.Clear();
        }
        
        internal void PlayAbilityFxCue(ref PlayAbilityFxCueContext context)
        {
            if (context.IsPersistent)
            {
                var hash = Animator.StringToHash(context.GameCueName);
                if (_persistentContexts.ContainsKey(hash))
                {
                    GameLogger.LogWarning($"Ability fx ue already played, ignoring. {context.GameCueName}");
                    return;
                }
                
                _persistentContexts.Add(hash, new PersistentAbilityCueContext()
                {
                    CueName = context.GameCueName,
                    UnitHandler = context.UnitHandler,
                    SubUnitHandler = context.SubUnitHandler,
                    Param = context.Param,
                });
            }
            
            _system.GameCueSubsystem.PlayAbilityFxCue(ref context);
        }

        internal void StopAbilityFxCue(ref StopAbilityFxCueContext context)
        {
            var hash = Animator.StringToHash(context.GameCueName);
            _persistentContexts.Remove(hash);
            
            _system.GameCueSubsystem.StopAbilityFxCue(ref context);
        }

        public void ReplayPersistentFx()
        {
            foreach (var pair in _persistentContexts)
            {
                var context = new PlayAbilityFxCueContext()
                {
                    GameCueName = pair.Value.CueName,
                    UnitHandler = pair.Value.UnitHandler,
                    SubUnitHandler = pair.Value.SubUnitHandler,
                    Param = pair.Value.Param,
                    AbilityHandler = _handler,
                    IsPersistent = true
                };
                _system.GameCueSubsystem.PlayAbilityFxCue(ref context);
            }
        }
    }
}