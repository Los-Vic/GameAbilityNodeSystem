using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class AbilityInstanceSubsystem:GameAbilitySubsystem
    {
        internal GameAbility CreateAbility(uint id)
        {
            var abilityAsset = System.AssetConfigProvider.GetAbilityAsset(id);
            if (abilityAsset == null)
            {
                GameLogger.LogError($"Fail to get ActiveAbilityAsset of {id}");
                return null;
            }
            
            var ability = System.GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Get<GameAbility>();
            var param = new AbilityCreateParam()
            {
                Asset = abilityAsset
            };
            
            ability.Init(System, ref param);
            GameLogger.Log($"Created Ability: {param.Asset.abilityName}");
            return ability;
        }

        internal void DestroyAbility(GameAbility ability)
        {
            GameLogger.Log($"Destroy Ability: {ability.Asset.abilityName}");
            System.GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Release(ability);
        }
    }
}