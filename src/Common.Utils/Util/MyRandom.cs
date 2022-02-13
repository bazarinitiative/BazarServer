using System.Numerics;
using System.Security.Cryptography;

namespace Common.Utils
{
	public static class MyRandom
	{
		/// <summary>
		/// 
		/// </summary>
		private static class RandomGen3
		{
			[ThreadStatic]
			private static Random? _local;

			public static int Next()
			{
				Random? inst = _local;
				if (inst == null)
				{
					byte[] buffer = RandomNumberGenerator.GetBytes(4);
					_local = inst = new Random(BitConverter.ToInt32(buffer, 0));
				}
				return inst.Next();
			}

			public static int Next(int min, int max)
			{
				Random? inst = _local;
				if (inst == null)
				{
					byte[] buffer = RandomNumberGenerator.GetBytes(4);
					_local = inst = new Random(BitConverter.ToInt32(buffer, 0));
				}
				return inst.Next(min, max);
			}
		}

		/// <summary>
		/// include min, not include max
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int Random(int min, int max)
		{
			return RandomGen3.Next(min, max);
		}

		/// <summary>
		/// return random result that None negative
		/// </summary>
		/// <returns></returns>
		public static int Random()
		{
			return RandomGen3.Next();
		}

		/// <summary>
		/// 1/rate return true, else return false
		/// </summary>
		/// <param name="rate"></param>
		/// <returns></returns>
		public static bool RandomRate(int rate)
		{
			int rest = Random() % rate;

			bool ret = (rest == 0);
			return ret;
		}

		/// <summary>
		/// return random string
		/// </summary>
		/// <param name="len"></param>
		/// <param name="chars">chars that may appear in result</param>
		/// <returns></returns>
		public static string RandomString(int len, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
		{
			var stringChars = new char[len];

			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = chars[Random() % chars.Length];
			}

			var finalString = new String(stringChars);
			return finalString;
		}

		/// <summary>
		/// return random Int128 that None negative
		/// </summary>
		/// <returns></returns>
		public static BigInteger Random128()
		{
			byte[] buf = new byte[16];
			for (int i = 0; i < buf.Length - 1; i++)
			{
				buf[i] = Convert.ToByte(Random(0, 255));
			}
			buf[^1] = Convert.ToByte(Random(0, 127));
			BigInteger ret = new BigInteger(buf);
			return ret;
		}

	}
}
