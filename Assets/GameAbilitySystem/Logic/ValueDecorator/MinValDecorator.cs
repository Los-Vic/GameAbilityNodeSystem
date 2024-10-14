using System;

namespace GameAbilitySystem.Logic.ValueDecorator
{
    public class MinValDecorator<T>:IValueDecorator<T> where T:IEquatable<T>, IComparable<T>
    {
        private readonly T _minVal;
        
        public MinValDecorator(T minVal)
        {
            _minVal = minVal;
        }
        
        public bool Process(in T inVal, out T outVal)
        {
            if (inVal.CompareTo(_minVal) >= 0)
            {
                outVal = inVal;
                return false;
            }

            outVal = _minVal;
            return true;
        }
    }
}