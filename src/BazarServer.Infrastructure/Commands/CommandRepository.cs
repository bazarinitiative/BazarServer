using BazarServer.Entity.Storage;
using Common.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BazarServer.Infrastructure.Commands
{
	public class CommandRepository : ICommandRepository
	{
		IGenericMongoCollection<UserCommand> _connCmd;

		IGenericMongoCollection<FailureCommand> _connFail;

		public CommandRepository(IGenericMongoCollection<UserCommand> conn, IGenericMongoCollection<FailureCommand> connFail)
		{
			_connCmd = conn;
			_connFail = connFail;
		}

		public async Task<(bool success, string msg)> SaveCommandAsync(UserCommand cmd)
		{
			await sem.WaitAsync();
			try
			{
				await _connCmd.UpsertAsync(x => x.commandID == cmd.commandID, cmd);
				return (true, "");
			}
			finally
			{
				sem.Release();
			}
		}

		public async Task<UserCommand?> GetCommandAsync(string commandID)
		{
			var ret = await _connCmd.FirstOrDefaultAsync(x => x.commandID == commandID);
			return ret;
		}

		public async Task<UserCommand?> GetCommandAsync_WithCache(string commandID, int cacheMilli)
		{
			var ret = await CacheHelper.WithCacheAsync(
											"command_" + commandID,
											async () => await GetCommandAsync(commandID),
											cacheMilli,
											false
											);
			return ret;
		}

		public async Task<List<UserCommand>> Batch(long lastOffset, int forwardCount)
		{
			var qry = _connCmd.GetQueryable()
									.Where(x => x.receiveOffset > lastOffset)
									.OrderBy(x => x.receiveOffset)
									.Take(forwardCount);
			var ret = await qry.ToListAsync();
			return ret;
		}

		public long GetLastReceiveOffset()
		{
			return lastReceiveOffset;
		}

		SemaphoreSlim sem = new SemaphoreSlim(1, 1);
		long lastReceiveOffset = -1;

		public async Task<long> GetNextReceiveOffset()
		{
			await sem.WaitAsync();
			try
			{
				if (lastReceiveOffset == -1)
				{
					var any = await _connCmd.GetQueryable().AnyAsync();
					if (!any)
					{
						lastReceiveOffset = 0;
					}
					else
					{
						lastReceiveOffset = await _connCmd.GetQueryable().MaxAsync(x => x.receiveOffset);
					}
				}
				lastReceiveOffset++;
				return lastReceiveOffset;
			}
			finally
			{
				sem.Release();
			}
		}

		public async Task SaveFailure(UserCommand cmd, string errMsg)
		{
			FailureCommand fc = new FailureCommand()
			{
				commandID = cmd.commandID,
				command = cmd,
				failTime = DateHelper.CurrentTimeMillis(),
				errMsg = errMsg
			};
			await _connFail.UpsertAsync(x => x.commandID == fc.commandID, fc);
		}
	}
}
