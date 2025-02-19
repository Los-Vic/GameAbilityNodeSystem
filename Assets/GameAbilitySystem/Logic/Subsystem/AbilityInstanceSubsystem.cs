using NS;

namespace GAS.Logic
{
    public class AbilityInstanceSubsystem:GameAbilitySubsystem
    {
        internal GameAbility CreateAbility(uint id)
        {
            var abilityAsset = System.AssetConfigProvider.GetAbilityAsset(id);
            if (abilityAsset == null)
            {
                System.Logger?.LogError($"Fail to get ActiveAbilityAsset of {id}");
                return null;
            }
            
            var ability = System.GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Get<GameAbility>();
            var param = new AbilityCreateParam()
            {
                Asset = abilityAsset
            };
            
            ability.Init(System, ref param);
            System.Logger?.Log($"Created Ability: {param.Asset.abilityName}");
            return ability;
        }

        internal void DestroyAbility(GameAbility ability)
        {
            System.Logger?.Log($"Destroy Ability: {ability.Asset.abilityName}");
            System.GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Release(ability);
        }
    }
}