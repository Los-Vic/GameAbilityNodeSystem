using MissQ;

namespace GAS.Logic
{
    public interface IAssetConfigProvider
    {
        public AbilityAsset GetAbilityAsset(uint abilityId);
        public FP GetAbilityEffectParamVal(string paramName, uint lv);
    }
}