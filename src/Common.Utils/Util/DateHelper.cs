using System.Diagnostics;

namespace Common.Utils
{
	public static class DateHelper
	{
		/// <summary>
		/// seconds since epoch
		/// </summary>
		/// <returns></returns>
		public static long CurrentTimeSeconds()
		{
			long ret = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			return ret;
		}

		/// <summary>
		/// milli seconds since Epoch
		/// </summary>
		/// <returns></returns>
		public static long CurrentTimeMillis()
		{
			long ret = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
			return ret;
		}

		/// <summary>
		/// milli seconds since Epoch
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static long TimeMillis(this DateTime dateTime)
		{
			long ret = (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
			return ret;
		}

		static Stopwatch? stopwatch = null;

		/// <summary>
		/// from specific time, for perf measure.
		/// </summary>
		/// <returns></returns>
		public static double GetMilliSeconds()
		{
			if (stopwatch == null)
			{
				stopwatch = new Stopwatch();
				stopwatch.Start();
			}
			return stopwatch.Elapsed.TotalMilliseconds;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamp"></param>
		/// <param name="timeZoneInfo"></param>
		/// <returns></returns>
		public static DateTime FromTimestamp(long timestamp, TimeZoneInfo timeZoneInfo)
		{
			if (timestamp < int.MaxValue)
			{
				DateTime dt = new DateTime(1970, 1, 1).AddSeconds(timestamp);
				var ret = TimeZoneInfo.ConvertTimeFromUtc(dt, timeZoneInfo);
				return ret;
			}
			else
			{
				DateTime dt = new DateTime(1970, 1, 1).AddMilliseconds(timestamp);
				var ret = TimeZoneInfo.ConvertTimeFromUtc(dt, timeZoneInfo);
				return ret;
			}

		}
	}
}
