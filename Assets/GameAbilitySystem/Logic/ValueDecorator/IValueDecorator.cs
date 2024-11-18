using System;
using MissQ;

namespace GAS.Logic
{
    public interface IValueDecorator
    {
        public bool Process(in FP inVal, out FP outVal);
    }
}