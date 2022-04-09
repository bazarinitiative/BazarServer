using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Storage;
using MongoDB.Driver;

namespace BazarServer.Infrastructure.Repository
{
	public class PeerServerRepository : IPeerServerRepository
	{
		IGenericMongoCollection<PeerServer> _conn;

		public PeerServerRepository(IGenericMongoCollection<PeerServer> conn)
		{
			_conn = conn;
		}

		public async Task<List<PeerServer>> GetAllAsync()
		{
			var ret = await _conn.GetQueryable().ToListAsync();
			return ret;
		}

		public List<PeerServer> GetAll()
		{
			return _conn.GetQueryable().ToList();
		}

		public async Task SaveAsync(PeerServer server)
		{
			await _conn.UpsertAsync(x => x.BaseUrl == server.BaseUrl, server);
		}

		public async Task RemoveAsync(string serverBaseUrl)
		{
			await _conn.RemoveAsync(x => x.BaseUrl == serverBaseUrl);
		}
	}
}
