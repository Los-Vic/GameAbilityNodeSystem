using System;
using MissQ;

namespace GAS.Logic
{
    [Serializable]
    public class MinValDecorator:ValueDecorator
    {
        public FP minVal;
        
        public MinValDecorator(FP minVal)
        {
            this.minVal = minVal;
        }
        
        public override bool Process(in FP inVal, out FP outVal)
        {
            if (inVal.CompareTo(minVal) >= 0)
            {
                outVal = inVal;
                return false;
            }

            outVal = minVal;
            return true;
        }
    }
}