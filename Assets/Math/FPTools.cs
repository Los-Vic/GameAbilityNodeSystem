/*
 * Copyright 2018 Tencent Inc.  
 *
 * Matthew  2017.12.25
 * 
 * 文件说明： FPTools.cs
 * FP Vector Quaternion Unity引擎与系统数据转换
 * 
 *
 */
using System;

namespace MissQ
{
	public static class FPTools
	{
#region Hash
		public static UInt32 HashOrigin()
		{
			return 2166136261U;
		}

		public static UInt32 HashPatchFP(UInt32 hash, FP num)
		{
			UInt64 raw = (UInt64)num.RawValue;
			for (int i = 0; i < 8; ++i)
			{
				byte b = (byte)((raw & (0xffUL << i)) >> i);
				hash ^= b;
				hash *= 16777619U;
			}
			return hash;
		}

		public static UInt32 HashPatchInt(UInt32 hash, int num)
		{
			for (int i = 0; i < 4; ++i)
			{
				byte b = (byte)((num & (0xffU << i)) >> i);
				hash ^= b;
				hash *= 16777619U;
			}
			return hash;
		}

        public static UInt32 HashPatchBytes(UInt32 hash,byte[] bNums)
        {
            if(bNums == null)
            {
                return hash;
            }
            for(int i = 0; i < bNums.Length; i++)
            {
                hash = HashPatchInt(hash,bNums[i]);
            }
            return hash;
        }

        #endregion

        #region Parse
        // 解析小数部分
        static bool TryParseDecimal(string str, out FP result)
		{
			result = 0;
			FP power = 1;
			for (int i = 0; i < str.Length; ++i)
			{
				power *= 10;
				char c = str[i];
				if (c >= '0' && c <= '9')
				{
					result += (c - '0') / power;
				}
				else
				{
					result = 0;
					return false;
				}
			}
			
			return true;
		}

		// 将字符串解析为定点数
		public static bool TryParse(string str, out FP result)
		{
			result = 0;
            str = str.TrimEnd('f', 'F');
			string[] splits = str.Split('.');
			if (splits.Length > 0)
			{
				int integer = 0;
				if (int.TryParse(splits[0], out integer))
				{
					bool negative = (integer < 0 || splits[0][0] == '-');
					if (splits.Length == 1)
					{
						result = integer;
						return true;
					}
					if (splits.Length == 2)
					{
						FP deci = 0;
						if (TryParseDecimal(splits[1], out deci))
						{
							if (negative)
							{
								result = integer - deci;
							}
							else
							{
								result = integer + deci;
							}
							return true;
						}
						else
						{
							result = 0;
							return false;
						}
					}
					else
					{
						result = 0;
						return false;
					}
				}
			}

			result = 0;
			return false;
		}

		// 将字符串解析为定点数
		public static FP Parse(string str)
		{
			FP result;
			if (TryParse(str, out result))
			{
				return result;
			}
			return FP.Zero;
		}
#endregion
		
		
        public static FP GetEulurY(TSVector forward)
		{
			forward.y = FP.Zero;

			if (forward.x == FP.Zero && forward.z == FP.Zero)
			{
				return FP.Zero;
			}

			if (TSMath.Abs(forward.z) < FP.EN3)
			{
				forward.z = 0;
			}

			FP re = FP.Atan2(forward.x, forward.z) * FP.Rad2Deg;
			return re;
		}
	}
}
