using System.Collections.Generic;

namespace GAS.Logic
{
    public class GameTagSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<EGameTag, GameTag> _tagInstances = new();
        public override void UnInit()
        {
            _tagInstances.Clear();
            base.UnInit();
        }

        internal GameTag GetGameTagInstance(EGameTag eGameTag)
        {
            if(_tagInstances.TryGetValue(eGameTag, out var tagInstance))
                return tagInstance;
            
            var newTagInstance = new GameTag(eGameTag);
            _tagInstances.Add(eGameTag, newTagInstance);
            return newTagInstance;
        }

        internal void AddGameTag(ITagOwner tagOwner, EGameTag eGameTag)
        {  
            if(tagOwner.GetTagContainer().HasTag(eGameTag))
                return;
            
            tagOwner.GetTagContainer().AddTag(eGameTag);
            var tagInstance = GetGameTagInstance(eGameTag);
            tagInstance.Owners.Add(tagOwner);
        }

        internal void RemoveGameTag(ITagOwner tagOwner, EGameTag eGameTag)
        {
            if(!tagOwner.GetTagContainer().HasTag(eGameTag))
                return;
            
            tagOwner.GetTagContainer().RemoveTag(eGameTag);
            var tagInstance = GetGameTagInstance(eGameTag);
            tagInstance.Owners.Remove(tagOwner);
        }
    }
}