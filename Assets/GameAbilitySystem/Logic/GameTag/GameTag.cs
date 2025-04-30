using System.Collections.Generic;

namespace GAS.Logic
{
    public class TagContainerComponent
    {
        private readonly HashSet<EGameTag> _tags = new();
        internal readonly ITagOwner Owner;

        public TagContainerComponent(ITagOwner o)
        {
            Owner = o;
        }
        
        public bool HasTag(EGameTag t) => _tags.Contains(t);
        public void AddTag(EGameTag t) => _tags.Add(t);
        public void RemoveTag(EGameTag t) => _tags.Remove(t);
    }
    
    public interface ITagOwner
    {
        TagContainerComponent GetTagContainer();
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