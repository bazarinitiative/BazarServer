using BazarServer.Entity.Storage;

namespace BazarServer.Infrastructure.Repository
{
	public interface IPeerServerRepository
	{
		List<PeerServer> GetAll();

		Task<List<PeerServer>> GetAllAsync();

		Task RemoveAsync(string serverBaseUrl);

		/// <summary>
		/// insert or update
		/// </summary>
		/// <param name="server"></param>
		/// <returns></returns>
		Task UpsertAsync(PeerServer server);
	}
}