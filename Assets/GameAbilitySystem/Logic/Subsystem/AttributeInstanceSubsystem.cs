using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    public class AttributeInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<ESimpleAttributeType, IAttributeValSetter> _attributeValSetters = new();

        public override void UnInit()
        {
            _attributeValSetters.Clear();
        }

        public void RegisterAttributeSetter(ESimpleAttributeType type, IAttributeValSetter setter)
        {
            _attributeValSetters.TryAdd(type, setter);
        }
        
        public void SetAttributeVal(GameUnit unit, ESimpleAttributeType type, FP newVal, GameEffect effect = null)
        {
            var attribute = unit.GetSimpleAttribute(type);
            if (attribute == null)
            {
                GameLogger.LogError($"fail to find simple attribute {type} of {unit.UnitName}");
                return;
            }
            
            var setter = _attributeValSetters.TryGetValue(attribute.Type, out var s)
                ? s : DefaultAttributeValSetter.Instance;
            
            setter.SetAttributeVal(unit, attribute, newVal, effect);
        }

        #region Attribute Instance Create/Destroy

        internal SimpleAttribute CreateSimpleAttribute(ref SimpleAttributeCreateParam param)
        {
            var attribute = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<SimpleAttribute>();
            attribute.Init(ref param);
            return attribute;
        }

        internal void DestroySimpleAttribute(SimpleAttribute attribute)
        {
            System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(attribute);
        }

        internal CompositeAttribute CreateCompositeAttribute(ref CompositeAttributeCreateParam param)
        {
            var attribute = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<CompositeAttribute>();
            attribute.Init(ref param);
            return attribute;
        }

        internal void DestroyCompositeAttribute(CompositeAttribute attribute)
        {
            System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(attribute);
        }

        #endregion
        
        
    }
}