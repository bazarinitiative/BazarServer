using System.Text;

namespace Common.Utils
{
	public static class StringExt
	{
		public static byte[] GetBytesUtf8(this string str)
		{
			return Encoding.UTF8.GetBytes(str);
		}

		public static string GetStringUtf8(this byte[] buf)
		{
			return Encoding.UTF8.GetString(buf);
		}

		/// <summary>
		/// return a substring of at most count, return str.length if not enough 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public static string Left(this string str, int count)
		{
			int len = Math.Min(str.Length, count);
			return str.Substring(0, len);
		}

		/// <summary>
		/// return a substring of at most count, return str.length if not enough 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static string Right(this string str, int count)
		{
			int len = Math.Min(str.Length, count);
			return str.Substring(str.Length - len, len);
		}

		/// <summary>
		/// return ToString() or return "" if obj is null
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string ToStringSupportNull(this object? obj)
		{
			return obj == null ? "" : obj.ToString() ?? "";
		}



		/// <summary>
		/// Returns the human-readable file size for an arbitrary, 64-bit file size 
		/// The default format is "0.# XB", e.g. "4.2 KB" or "1.4 GB"
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string ToHumanBytes(this long val)
		{
			// Get absolute value
			long absolute_i = val < 0 ? -val : val;
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = val >> 50;
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = val >> 40;
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = val >> 30;
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = val >> 20;
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = val >> 10;
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = val;
			}
			else
			{
				return val.ToString("0 B"); // Byte
			}
			// Divide by 1024 to get fractional value
			readable /= 1024;
			// Return formatted number with suffix
			return readable.ToString("0.# ") + suffix;
		}

		/// <summary>
		/// Returns the human-readable number 
		/// The default format is "0.# X", e.g. "14.2 K" or "1.4 M"
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string ToHumanNumber(this int val)
		{
			// Get absolute value
			long absolute_i = val < 0 ? -val : val;
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 1000_000) // million
			{
				suffix = "M";
				readable = val / 1000;
			}
			else if (absolute_i >= 1000) // K
			{
				suffix = "K";
				readable = val;
			}
			else
			{
				return val.ToString();
			}
			readable /= 1000;
			var prefix = "";
			if (val < 0)
			{
				prefix = "-";
			}
			// Return formatted number with suffix
			return prefix + readable.ToString("0.# ") + suffix;
		}

		/// <summary>
		/// every char is letter or digit
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static bool IsLetterDigitAll(this string str)
		{
			foreach (var item in str)
			{
				if (!char.IsLetterOrDigit(item))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// every char is letter or digit, and length is 30
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static bool IsLetterDigit30(this string str)
		{
			if (str.Length != 30)
			{
				return false;
			}
			if (!str.IsLetterDigitAll())
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// return Json.Serialize(obj)
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string ToJsonString(this object? obj)
		{
			var ret = Json.Serialize(obj);
			return ret;
		}
	}
}
