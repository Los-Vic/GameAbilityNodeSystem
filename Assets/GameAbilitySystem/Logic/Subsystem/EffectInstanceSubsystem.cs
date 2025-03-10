using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class EffectInstanceSubsystem:GameAbilitySubsystem
    {
        #region Effect Instance Create/Destroy

        internal GameEffect CreateEffect(uint id)
        {
            var effectAsset = System.AssetConfigProvider.GetEffectAsset(id);
            if (effectAsset == null)
            {
                GameLogger.LogError($"Fail to get EffectAbilityAsset of {id}");
                return null;
            }
            
            var effect = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<GameEffect>();
            var param = new EffectCreateParam()
            {
                Asset = effectAsset
            };
            
            effect.Init(System, ref param);
            return effect;
        }
        
        internal void DestroyAbility(GameEffect effect)
        {
            
        }

        #endregion
    }
}