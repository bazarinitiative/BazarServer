namespace BazarServer.Application.PeerServers
{
	public class PeerServerDto
	{
		/// <summary>
		/// each peerServer need an unique baseUrl like "https://www.yourEntity.com/bazarserver/" or something
		/// </summary>
		public string baseUrl { get; set; }

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

		public PeerServerDto(string baseUrl)
		{
			this.baseUrl = baseUrl;
		}

	}
}
