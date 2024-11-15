using System.Collections.Generic;

namespace GameAbilitySystem.Logic
{
    public class TagContainer
    {
        internal readonly Dictionary<GameAbilitySystemCfg.EGameTag, GameTag> Tags = new();
        internal readonly ITagOwner Owner;

        public TagContainer(ITagOwner o)
        {
            Owner = o;
        }
    }
    
    public interface ITagOwner
    {
        TagContainer GetTagContainer();
        bool HasTag(GameAbilitySystemCfg.EGameTag t);
    }
    
    public class GameTag
    {
        internal readonly GameAbilitySystemCfg.EGameTag EffectTagEnum;
        internal readonly List<ITagOwner> Owners = new();

        internal GameTag(GameAbilitySystemCfg.EGameTag t)
        {
            EffectTagEnum = t;
        }
    }
}