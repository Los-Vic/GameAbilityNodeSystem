using System;

namespace GameAbilitySystem.Logic
{
    public interface IValueDecorator<T>
    {
        public bool Process(in T inVal, out T outVal);
    }
}