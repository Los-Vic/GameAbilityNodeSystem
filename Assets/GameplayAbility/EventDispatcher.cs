using System;
using System.Collections.Generic;
using Gameplay.Common;

namespace Gameplay.Ability
{
    //Events of all tables for the outer world
    public class EventDispatcher
    {
        private Dictionary<(EUnitPrimaryAttribute, uint), Event<float, float>> _onUnitPrimaryAttributeUpdateHooks;
        private Dictionary<(EUnitSecondaryAttribute, uint), Event<float, float>> _onUnitSecondaryAttributeUpdateHooks;


        public void RegisterUnitPrimaryAttributeUpdate(EUnitPrimaryAttribute attributeType, uint unitID,
            Action<float, float> hook)
        {
            if (!_onUnitPrimaryAttributeUpdateHooks.TryGetValue((attributeType, unitID), out var evt))
            {
                evt = new Event<float, float>();
            }

            evt.Register(hook);
        }

        public void UnregisterUnitPrimaryAttributeUpdate(EUnitPrimaryAttribute attributeType, uint unitID, Action<float, float> hook)
        {
            if (!_onUnitPrimaryAttributeUpdateHooks.TryGetValue((attributeType, unitID), out var evt))
                return;
            evt.Unregister(hook);
        }
        
        
        internal void Init(World world)
        {
            _onUnitPrimaryAttributeUpdateHooks = new();
            _onUnitSecondaryAttributeUpdateHooks = new();
        }

        internal void UnInit()
        {
            _onUnitPrimaryAttributeUpdateHooks.Clear();
            _onUnitSecondaryAttributeUpdateHooks.Clear();
        }

        internal void InternalOnPrimaryAttributeUpdate(EUnitPrimaryAttribute attributeType, uint unitID, float oldVal,
            float newVal)
        {
            if(!_onUnitPrimaryAttributeUpdateHooks.TryGetValue((attributeType, unitID), out var evt))
                return;
            evt.Broadcast(oldVal, newVal);
        }
        
        internal void InternalOnSecondaryAttributeUpdate(EUnitSecondaryAttribute attributeType, uint unitID, float oldVal,
            float newVal)
        {
            if(!_onUnitSecondaryAttributeUpdateHooks.TryGetValue((attributeType, unitID), out var evt))
                return;
            evt.Broadcast(oldVal, newVal);
        }
    }
}