namespace BazarServer.Entity.PeerServers
{
	public interface IPeerServerRepository
	{
		List<PeerServer> GetAll();
		Task<List<PeerServer>> GetAllAsync();
		Task RemoveAsync(string serverBaseUrl);
		Task SaveAsync(PeerServer server);
	}
}