namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
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
    }
}