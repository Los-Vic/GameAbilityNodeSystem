using System;
using System.Collections.Generic;
using GameAbilitySystem.Logic.Effect;
using GameAbilitySystem.Logic.Unit;

namespace GameAbilitySystem.Logic.Attribute
{
    public class AttributeInstanceMgr<T> where T:IEquatable<T>, IComparable<T>
    {
        private readonly GameAbilitySystem<T> _system;
        private readonly Dictionary<GameAbilitySystemCfg.ESimpleAttributeType, IAttributeValSetter<T>> _attributeValSetters = new();

        public AttributeInstanceMgr(GameAbilitySystem<T> sys)
        {
            _system = sys;
        }

        public void RegisterAttributeSetter(GameAbilitySystemCfg.ESimpleAttributeType type, IAttributeValSetter<T> setter)
        {
            _attributeValSetters.TryAdd(type, setter);
        }
        
        public void SetAttributeVal(GameUnit<T> unit, SimpleAttribute<T> attribute, T newVal, GameEffect<T> effect = null)
        {
            var setter = _attributeValSetters.TryGetValue(attribute.Type, out var s)
                ? s : DefaultAttributeValSetter<T>.Instance;
            
            setter.SetAttributeVal(unit, attribute, newVal, effect);
        }

        #region Attribute Instance Create/Destroy

        internal SimpleAttribute<T> CreateSimpleAttribute(ref SimpleAttributeCreateParam<T> param)
        {
            var attribute = _system.ObjectPoolMgr.CreateObject<SimpleAttribute<T>>();
            attribute.Init(ref param);
            return attribute;
        }

        internal void DestroySimpleAttribute(SimpleAttribute<T> attribute)
        {
            _system.ObjectPoolMgr.DestroyObject(attribute);
        }

        internal CompositeAttribute<T> CreateCompositeAttribute(ref CompositeAttributeCreateParam<T> param)
        {
            var attribute = _system.ObjectPoolMgr.CreateObject<CompositeAttribute<T>>();
            attribute.Init(ref param);
            return attribute;
        }

        internal void DestroyCompositeAttribute(CompositeAttribute<T> attribute)
        {
            _system.ObjectPoolMgr.DestroyObject(attribute);
        }

        #endregion
        
        
    }
}