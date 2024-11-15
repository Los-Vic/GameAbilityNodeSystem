namespace MissQ
{
    public static class FPGameMath
    {
        //通常策划只要求万分之一的精度
        public const float IntegerEpsilon = 0.00001f;

        //判断是否在精度范围内为整数
        public static bool IsInteger(FP value, out FP result)
        {
            result = value;
            var fractionalPart = value._serializedValue & 0x00000000FFFFFFFF;
            var fraction = new FP(fractionalPart);
            var integer = new FP(value._serializedValue - fractionalPart);
            
            if (fraction <= IntegerEpsilon)
            {
                result = integer;
                return true;
            }
            
            if (fraction >= 1 - IntegerEpsilon)
            {
                result = integer + (integer > 0 ? 1 : -1) * FP.One;
                return true;
            }
            
            return false;
        }

        //返回不大于给定数值的整数
        public static FP Floor(FP value)
        {
            return IsInteger(value, out var res) ? res : new FP((long)((ulong)value._serializedValue & 0xFFFFFFFF00000000));
        }
        
        //返回不小于给定数值的整数
        public static FP Ceiling(FP value)
        {
            if (IsInteger(value, out var res))
                return res;
            
            var integer = new FP((long)((ulong)value._serializedValue & 0xFFFFFFFF00000000));
            return integer + FP.One;
        }

        //四舍五入
        public static FP Round(FP value)
        {
            if (IsInteger(value, out var res))
                return res;
            
            var fractionalPart = value._serializedValue & 0x00000000FFFFFFFF;
            var integer = Floor(value);
           
            if (fractionalPart < 0x80000000)
            {
                return integer;
            }

            return integer + FP.One;
        }
        
        //四舍六入五成双
        public static FP Round2(FP value)
        {
            if (IsInteger(value, out var res))
                return res;
            
            var fractionalPart = value._serializedValue & 0x00000000FFFFFFFF;
            var integer = Floor(value);
           
            if (fractionalPart < 0x80000000)
            {
                return integer;
            }

            if (fractionalPart > 0x80000000)
            {
                return integer + FP.One; 
            }
            
            return (integer._serializedValue & FP.ONE) == 0
                ? integer
                : integer + FP.One;
        }
    }
}