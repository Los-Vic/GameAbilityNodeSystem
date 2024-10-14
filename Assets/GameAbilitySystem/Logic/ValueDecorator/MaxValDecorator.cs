using System;

namespace GameAbilitySystem.Logic.ValueDecorator
{
    public class MaxValDecorator<T>:IValueDecorator<T> where T:IEquatable<T>, IComparable<T>
    {
        private readonly T _maxVal;
        
        public MaxValDecorator(T maxVal)
        {
            _maxVal = maxVal;
        }
        
        public bool Process(in T inVal, out T outVal)
        {
            if (inVal.CompareTo(_maxVal) <= 0)
            {
                outVal = inVal;
                return false;
            }

            outVal = _maxVal;
            return true;
        }
    }
}