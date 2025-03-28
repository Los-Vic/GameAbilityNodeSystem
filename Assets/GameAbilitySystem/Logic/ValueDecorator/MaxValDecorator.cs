using MissQ;

namespace GAS.Logic
{
    public class MaxValDecorator:IValueDecorator
    {
        private readonly FP _maxVal;
        
        public MaxValDecorator(FP maxVal)
        {
            _maxVal = maxVal;
        }
        
        public bool Process(in FP inVal, out FP outVal)
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