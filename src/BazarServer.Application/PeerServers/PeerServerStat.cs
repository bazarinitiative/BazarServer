using BazarServer.Entity.Storage;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.PeerServers
{
	public class PeerServerStat
	{
		public PeerServer server { get; set; }

		ILogger logger;
		PeerManager owner;

		public PeerServerStat(PeerManager owner, PeerServer server, ILogger logger)
		{
			this.server = server;

			this.logger = logger;
			this.owner = owner;
		}

	}

}
