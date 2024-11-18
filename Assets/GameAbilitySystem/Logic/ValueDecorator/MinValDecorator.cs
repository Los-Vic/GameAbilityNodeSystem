using System;
using MissQ;

namespace GAS.Logic
{
    public class MinValDecorator:IValueDecorator
    {
        private readonly FP _minVal;
        
        public MinValDecorator(FP minVal)
        {
            _minVal = minVal;
        }
        
        public bool Process(in FP inVal, out FP outVal)
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