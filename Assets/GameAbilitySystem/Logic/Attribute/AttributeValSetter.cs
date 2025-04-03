using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    public interface IAttributeValSetter
    {
        void SetAttributeVal(GameUnit unit, SimpleAttribute attribute, FP newVal, GameEffect changedByEffect = null);
    }

    public class DefaultAttributeValSetter : IAttributeValSetter
    {
        public static readonly DefaultAttributeValSetter Instance = new();
        private DefaultAttributeValSetter(){}
        
        public void SetAttributeVal(GameUnit unit, SimpleAttribute attribute, FP newVal, GameEffect changedByEffect = null)
        {
            var oldVal = attribute.Val;
            if(newVal.Equals(oldVal))
                return;
            
            attribute.SetVal(newVal);

            GameLogger.Log($"DefaultAttributeValSetter set val, unit {unit.UnitName}, attribute {attribute.Type}, inNewVal {newVal}, " +
                           $"res {oldVal}->{attribute.Val}, changedByEffect {changedByEffect?.EffectName ?? ""}");
            
            if (attribute.Val != oldVal)
            {
                attribute.OnValChanged.NotifyObservers(new AttributeChangeMsg()
                {
                    OldVal = oldVal,
                    NewVal = attribute.Val,
                    ChangedByEffect = changedByEffect
                });
            }
        }
    }
    
    public class DefaultAttributeNoLogValSetter : IAttributeValSetter
    {
        public static readonly DefaultAttributeNoLogValSetter Instance = new();
        private DefaultAttributeNoLogValSetter(){}
        
        public void SetAttributeVal(GameUnit unit, SimpleAttribute attribute, FP newVal, GameEffect changedByEffect = null)
        {
            var oldVal = attribute.Val;
            if(newVal.Equals(oldVal))
                return;
            
            attribute.SetVal(newVal);
            if (attribute.Val != oldVal)
            {
                attribute.OnValChanged.NotifyObservers(new AttributeChangeMsg()
                {
                    OldVal = oldVal,
                    NewVal = attribute.Val,
                    ChangedByEffect = changedByEffect
                });
            }
        }
    }
    
}