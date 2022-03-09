using System.Collections.Concurrent;

namespace Common.Utils
{
	/// <summary>
	/// this class is thread safe
	/// </summary>
	public class FrequencyControl
	{
		long seconds = 0;
		int maxAllowQuantity;

		public FrequencyControl(long seconds_, int maxAllowQuantity_)
		{
			if (seconds_ < 1)
			{
				throw new Exception("time span should at least 1 second");
			}
			seconds = seconds_;
			maxAllowQuantity = maxAllowQuantity_;
		}

		class KeyUnit
		{
			public KeyUnit(string key_, long seconds_, int maxAllowQuantity_)
			{
				key = key_;
				seconds = seconds_;
				maxAllowQuantity = maxAllowQuantity_;
			}

			long seconds;
			int maxAllowQuantity;

			string key;
			List<long> checkHistory = new List<long>(); //fist at pos [0]

			void Trim()
			{
				long timestampNow = DateHelper.CurrentTimeSeconds();

				long timestampStart = timestampNow - seconds;

				for (int i = 0; i < checkHistory.Count; i++)
				{
					if (checkHistory[0] < timestampStart)
					{
						checkHistory.RemoveAt(0);
					}
					else
					{
						break;
					}
				}
			}

			public bool Check()
			{
				lock (this)
				{
					this.Trim();

					if (checkHistory.Count >= maxAllowQuantity)
					{
						return false;
					}
					else
					{
						long timestampNow = DateHelper.CurrentTimeSeconds();
						checkHistory.Add(timestampNow);
						return true;
					}

				}
			}

			public int GetCurrentCount()
			{
				lock (this)
				{
					this.Trim();
					return checkHistory.Count;
				}
			}
		}

		ConcurrentDictionary<string, KeyUnit> allKeys = new ConcurrentDictionary<string, KeyUnit>();

		KeyUnit EnsureNode(string key)
		{
			var ret = allKeys.GetOrAdd(key, new KeyUnit(key, seconds, maxAllowQuantity));
			return ret;
		}

		public bool Check(string key)
		{
			var node = EnsureNode(key);
			return node.Check();
		}

		public int GetCurrentCount(string key)
		{
			var node = EnsureNode(key);
			return node.GetCurrentCount();
		}

		public (int maxAllowQuantity, long seconds) GetLimit()
		{
			return (maxAllowQuantity, seconds);
		}

		/// <summary>
		/// try remove empty KeyUnit to save memory
		/// </summary>
		public void Trim()
		{
			var keys = allKeys.Keys.ToList();
			foreach (var key in keys)
			{
				if (allKeys.TryGetValue(key, out var unit))
				{
					if (unit.GetCurrentCount() == 0)
					{
						allKeys.TryRemove(key, out _);
					}
				}
			}
		}
	}
}
