using BazarServer.Application.PeerServers;

namespace BazarServer.Services
{
	public class PeerService : IHostedService
	{
		IPeerManager peerManager;

		public PeerService(IPeerManager peerManager)
		{
			this.peerManager = peerManager;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			peerManager.Start();
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			peerManager.Stop();
			return Task.CompletedTask;
		}
	}
}
