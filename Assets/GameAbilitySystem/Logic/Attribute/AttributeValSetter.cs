using System;
using MissQ;

namespace GameAbilitySystem.Logic
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
            attribute.OnValChanged.NotifyObservers(new AttributeChangeMsg()
            {
                OldVal = oldVal,
                NewVal = newVal,
                ChangedByEffect = changedByEffect
            });
        }
    }
}