using System.Collections.Generic;

namespace GAS.Logic
{
    public class TagContainer
    {
        internal readonly Dictionary<EGameTag, GameTag> Tags = new();
        internal readonly ITagOwner Owner;

        public TagContainer(ITagOwner o)
        {
            Owner = o;
        }
    }
    
    public interface ITagOwner
    {
        TagContainer GetTagContainer();
        bool HasTag(EGameTag t);
    }
    
    public class GameTag
    {
        internal readonly EGameTag EffectTagEnum;
        internal readonly List<ITagOwner> Owners = new();

        internal GameTag(EGameTag t)
        {
            EffectTagEnum = t;
        }
    }
}