using System;
using MissQ;

namespace GAS.Logic
{
    [Serializable]
    public class ValueDecorator
    {
        public virtual bool Process(in FP inVal, out FP outVal)
        {
            outVal = inVal;
            return true;
        }
    }
}