using System;
using MissQ;

namespace GameAbilitySystem.Logic
{
    public interface IValueDecorator
    {
        public bool Process(in FP inVal, out FP outVal);
    }
}