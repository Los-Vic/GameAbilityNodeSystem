using System;
using MissQ;

namespace GAS.Logic
{
    [Serializable]
    public class MaxValDecorator:ValueDecorator
    {
        public FP maxVal;
        
        public MaxValDecorator(FP maxVal)
        {
            this.maxVal = maxVal;
        }
        
        public override bool Process(in FP inVal, out FP outVal)
        {
            if (inVal.CompareTo(maxVal) <= 0)
            {
                outVal = inVal;
                return false;
            }

            outVal = maxVal;
            return true;
        }
    }
}