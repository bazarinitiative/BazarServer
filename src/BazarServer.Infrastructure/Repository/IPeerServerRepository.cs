using BazarServer.Entity.Storage;

namespace BazarServer.Infrastructure.Repository
{
	public interface IPeerServerRepository
	{
		List<PeerServer> GetAll();
		Task<List<PeerServer>> GetAllAsync();
		Task RemoveAsync(string serverBaseUrl);
		Task SaveAsync(PeerServer server);
	}
}