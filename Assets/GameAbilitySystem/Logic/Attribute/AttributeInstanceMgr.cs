using System;
using System.Collections.Generic;
using MissQ;

namespace GameAbilitySystem.Logic
{
    public class AttributeInstanceMgr 
    {
        private readonly GameAbilitySystem _system;
        private readonly Dictionary<GameAbilitySystemCfg.ESimpleAttributeType, IAttributeValSetter> _attributeValSetters = new();

        public AttributeInstanceMgr(GameAbilitySystem sys)
        {
            _system = sys;
        }

        public void RegisterAttributeSetter(GameAbilitySystemCfg.ESimpleAttributeType type, IAttributeValSetter setter)
        {
            _attributeValSetters.TryAdd(type, setter);
        }
        
        public void SetAttributeVal(GameUnit unit, SimpleAttribute attribute, FP newVal, GameEffect effect = null)
        {
            var setter = _attributeValSetters.TryGetValue(attribute.Type, out var s)
                ? s : DefaultAttributeValSetter.Instance;
            
            setter.SetAttributeVal(unit, attribute, newVal, effect);
        }

        #region Attribute Instance Create/Destroy

        internal SimpleAttribute CreateSimpleAttribute(ref SimpleAttributeCreateParam param)
        {
            var attribute = _system.ObjectPoolMgr.CreateObject<SimpleAttribute>();
            attribute.Init(ref param);
            return attribute;
        }

        internal void DestroySimpleAttribute(SimpleAttribute attribute)
        {
            _system.ObjectPoolMgr.DestroyObject(attribute);
        }

        internal CompositeAttribute CreateCompositeAttribute(ref CompositeAttributeCreateParam param)
        {
            var attribute = _system.ObjectPoolMgr.CreateObject<CompositeAttribute>();
            attribute.Init(ref param);
            return attribute;
        }

        internal void DestroyCompositeAttribute(CompositeAttribute attribute)
        {
            _system.ObjectPoolMgr.DestroyObject(attribute);
        }

        #endregion
        
        
    }
}