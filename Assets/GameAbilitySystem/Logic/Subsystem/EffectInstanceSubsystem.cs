

namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
        #region Effect Instance Create/Destroy

        internal GameEffect CreateEffect<T>(string effectName) where T : GameEffect, new()
        {
            var effect = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<T>();
            effect.Init(effectName);
            return effect;
        }
        
        internal void DestroyEffect(GameEffect effect)
        {
            effect.GetRefCountDisposableComponent().MarkForDispose();
        }

        #endregion
    }
}