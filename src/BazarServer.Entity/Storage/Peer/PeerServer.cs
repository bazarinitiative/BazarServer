
namespace BazarServer.Entity.Storage
{
	public class PeerServer : IStoreData
	{
		/// <summary>
		/// such as https://api.yourdoman.com/bazar/
		/// </summary>
		public string BaseUrl { get; set; } = "";

		/// <summary>
		/// 1 is max
		/// </summary>
		public double Reputation { get; set; } = 0.5;

		/// <summary>
		/// receive command count from peer. include dup and error
		/// </summary>
		public long ReceiveCount { get; set; }

		/// <summary>
		/// already received from user or other peer
		/// </summary>
		public long ReceiveDupCount { get; set; }

		/// <summary>
		/// fail deserialize, no user, signature fail, spam fail, etc
		/// </summary>
		public long ReceiveErrorCount { get; set; }

		/// <summary>
		/// receive command count from peer. effective ones only.
		/// </summary>
		public long ReceiveOkCount { get; set; }

		/// <summary>
		/// offset of ReceiveTime for this peer
		/// </summary>
		public long lastReceiveOffset { get; set; }

		/// <summary>
		/// Time of next retrieve.
		/// </summary>
		public DateTime nextRetrieveTime { get; set; }

		public void IncreaseReputation(int rate)
		{
			var change = (1 - Reputation) / rate;
			Reputation += change;
		}

		public void ReduceReputation(int rate)
		{
			var change = Reputation / rate;
			Reputation -= change;
		}

	}
}

