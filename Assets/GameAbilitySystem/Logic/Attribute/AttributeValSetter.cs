using System;

namespace GameAbilitySystem.Logic
{
    public interface IAttributeValSetter<T> where T:IEquatable<T>, IComparable<T>
    {
        void SetAttributeVal(GameUnit<T> unit, SimpleAttribute<T> attribute, T newVal, GameEffect<T> changedByEffect = null);
    }

    public class DefaultAttributeValSetter<T> : IAttributeValSetter<T> where T:IEquatable<T>, IComparable<T>
    {
        public static readonly DefaultAttributeValSetter<T> Instance = new();
        private DefaultAttributeValSetter(){}
        
        public void SetAttributeVal(GameUnit<T> unit, SimpleAttribute<T> attribute, T newVal, GameEffect<T> changedByEffect = null)
        {
            var oldVal = attribute.Val;
            if(newVal.Equals(oldVal))
                return;
            attribute.SetVal(newVal);
            attribute.OnValChanged.NotifyObservers(new AttributeChangeMsg<T>()
            {
                OldVal = oldVal,
                NewVal = newVal,
                ChangedByEffect = changedByEffect
            });
        }
    }
}