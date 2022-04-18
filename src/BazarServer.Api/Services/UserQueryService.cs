using BazarServer.Application.Query;
using BazarServer.Infrastructure.Repository;

namespace BazarServer.Services
{
	/// <summary>
	/// background service for user query related logic
	/// </summary>
	public class UserQueryService : IHostedService
	{
		Thread? thread = null;
		bool running = true;
		ICommandRepository commandRepository;
		IPostRepository postRepository;
		ILogger<UserQueryService> logger;

		public UserQueryService(ICommandRepository commandRepository, IPostRepository postRepository, ILogger<UserQueryService> logger)
		{
			this.commandRepository = commandRepository;
			this.postRepository = postRepository;
			this.logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			running = true;
			thread = new Thread(async () => await Worker());
			thread.Start();

			return Task.CompletedTask;
		}

		async Task Worker()
		{
			var lastReceiveOffset = commandRepository.GetLastReceiveOffset();

			while (running)
			{
				try
				{
					var list = await commandRepository.Batch(lastReceiveOffset, 1000);
					foreach (var cmd in list)
					{
						if (cmd.commandType == "Post" || cmd.commandType == "Delete")
						{
							PostQueryFacade.RemoveUserLatestPostsCache(cmd.userID);
						}
						if (cmd.receiveOffset > lastReceiveOffset)
						{
							lastReceiveOffset = cmd.receiveOffset;
						}
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex.ToString());
					await Task.Delay(1000);
				}

				await Task.Delay(50);
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			running = false;
			if (thread != null)
			{
				thread.Join();
				thread = null;
			}
			return Task.CompletedTask;
		}
	}
}
