
namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
        #region Effect Instance Create/Destroy

        internal GameEffect CreateEffect(EffectCreateParam param)
        {
            var effect = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<GameEffect>();
            
            effect.Init(System, ref param);
            return effect;
        }
        
        internal void DestroyAbility(GameEffect effect)
        {
            
        }

        #endregion
    }
}