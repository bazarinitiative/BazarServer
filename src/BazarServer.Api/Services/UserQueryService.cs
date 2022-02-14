using BazarServer.Application.Posts;
using BazarServer.Entity.Storage;

namespace BazarServer.Services
{
	/// <summary>
	/// background service for user query related logic
	/// </summary>
	public class UserQueryService : IHostedService
	{
		bool running = true;
		ICommandRepository commandRepository;
		IPostRepository postRepository;

		public UserQueryService(ICommandRepository commandRepository, IPostRepository postRepository)
		{
			this.commandRepository = commandRepository;
			this.postRepository = postRepository;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			running = true;

			Thread thread = new Thread(async () => await Worker());
			thread.Start();

			return Task.CompletedTask;
		}

		async Task Worker()
		{
			var lastReceiveOffset = commandRepository.GetLastReceiveOffset();

			while (running)
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
				await Task.Delay(50);
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			running = false;
			return Task.CompletedTask;
		}
	}
}
