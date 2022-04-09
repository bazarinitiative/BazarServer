using BazarServer.Entity.Storage;

namespace BazarServer.Infrastructure.Repository
{
	public interface ICommandRepository
	{

		/// <summary>
		/// with cache. null will not cache.
		/// </summary>
		/// <param name="commandID"></param>
		/// <param name="cacheMilli"></param>
		/// <returns></returns>
		Task<UserCommand?> GetCommandAsync_WithCache(string commandID, int cacheMilli = 600 * 1000);

		/// <summary>
		/// no memcache
		/// </summary>
		/// <param name="commandID"></param>
		/// <returns></returns>
		public Task<UserCommand?> GetCommandAsync(string commandID);

		/// <summary>
		/// save command to mongodb
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		Task<(bool success, string msg)> SaveCommandAsync(UserCommand cmd);

		/// <summary>
		/// return those command.receiveOffset > lastOffset, max count forwardCount
		/// result will not include lastOffset.
		/// </summary>
		/// <param name="lastOffset"></param>
		/// <param name="forwardCount"></param>
		/// <returns></returns>
		Task<List<UserCommand>> Batch(long lastOffset, int forwardCount);

		/// <summary>
		/// every command will be assigned a increasing receiveOffset
		/// </summary>
		/// <returns></returns>
		Task<long> GetNextReceiveOffset();

		/// <summary>
		/// on error, save command to failure table
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="errMsg"></param>
		/// <returns></returns>
		Task SaveFailure(UserCommand cmd, string errMsg);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		long GetLastReceiveOffset();
	}
}