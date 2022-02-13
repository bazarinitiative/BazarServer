using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace Common.Utils
{
	public class AntiSpam : IAntiSpam
	{
		ConcurrentDictionary<string, ConcurrentDictionary<string, FrequencyControl>> dic = new ConcurrentDictionary<string, ConcurrentDictionary<string, FrequencyControl>>();

		public AntiSpam(IConfiguration configuration)
		{
			var anti = configuration.GetSection("AntiSpam");
			foreach (var item in anti.GetChildren())
			{
				try
				{
					var key = item.Key;
					var limits = item.Get<string[]>();
					var subdic = dic.GetOrAdd(key, new ConcurrentDictionary<string, FrequencyControl>());
					foreach (var limit in limits)
					{
						var ay = limit.Split(',');
						int max = Convert.ToInt32(ay[0]);
						long seconds = Convert.ToInt64(ay[1]);
						subdic.TryAdd(limit, new FrequencyControl(seconds, max));
					}
				}
				catch (Exception ex)
				{
					var msg = $"fail to parse: {item.Key} {item.Value}";
					throw new Exception(msg, ex);
				}
			}
		}

		public (bool success, string msg) Check(string type, string spamKey)
		{
			dic.TryGetValue(type, out var fcs);
			if (fcs == null)
			{
				throw new Exception("unknow spamType");
			}
			foreach (var key in fcs.Keys)
			{
				var fc = fcs[key];
				if (!fc.Check(spamKey))
				{
					var limit = fc.GetLimit();
					return (false, $"exceed {limit.maxAllowQuantity} in {limit.seconds} seconds");
				}
			}
			return (true, "");
		}
	}
}
