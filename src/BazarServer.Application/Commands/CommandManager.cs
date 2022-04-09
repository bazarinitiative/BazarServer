using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Commands
{
	public class CommandManager : ICommandManager
	{
		ICommandRepository commandRepository;
		ILogger<CommandManager> logger;
		ISender mediator;

		public CommandManager(ICommandRepository commandRepository, ILogger<CommandManager> logger, ISender mediator)
		{
			this.commandRepository = commandRepository;
			this.logger = logger;
			this.mediator = mediator;
		}

		public async Task<MdtResp> SaveAndDispatch(UserCommand cmdOrig, string commandFrom)
		{
			var cmd = new UserCommand();
			FastCopy.Copy(cmdOrig, cmd);

			cmd.receiveOffset = await commandRepository.GetNextReceiveOffset();
			var sc = await commandRepository.SaveCommandAsync(cmd);
			if (!sc.success)
			{
				logger.LogError($"fail to save command: {sc.msg}");
				return new MdtResp(false, $"fail to save command");
			}

			//we save command before process it, so every command will be processed only once.
			//you can manually rescue them from failure if you want.

			try
			{
				MdtResp resp;
				switch (cmd.commandType)
				{
					case "UserInfo":
						resp = await OnUserCommandDetail<UserInfoCmd>(cmd, commandFrom);
						break;
					case "UserPic":
						resp = await OnUserCommandDetail<UserPicCmd>(cmd, commandFrom);
						break;
					case "Following":
						resp = await OnUserCommandDetail<FollowingCmd>(cmd, commandFrom);
						break;
					case "Channel":
						resp = await OnUserCommandDetail<ChannelCmd>(cmd, commandFrom);
						break;
					case "Post":
						resp = await OnUserCommandDetail<PostCmd>(cmd, commandFrom);
						break;
					case "Like":
						resp = await OnUserCommandDetail<LikeCmd>(cmd, commandFrom);
						break;
					case "Delete":
						resp = await OnUserCommandDetail<DeleteCmd>(cmd, commandFrom);
						break;
					default:
						//ignore unknow command
						var msg = $"ignore unkow userCommandType: {cmd.commandType}";
						logger.LogInformation(msg);
						resp = new MdtResp(false, msg);
						break;
				}

				if (!resp.success)
				{
					await SaveFailure(cmd, resp.msg);
				}

				return resp;
			}
			catch (Exception ex)
			{
				await SaveFailure(cmd, ex.ToString());

				logger.LogError(ex, $"cmd={cmd.ToJsonString()}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task SaveFailure(UserCommand cmd, string errMsg)
		{
			try
			{
				await commandRepository.SaveFailure(cmd, errMsg);
			}
			catch (Exception)
			{
			}
		}

		private async Task<MdtResp> OnUserCommandDetail<T>(UserCommand cmd, string commandFrom) where T : ICommandContent
		{
			var ret = MdtRequest<T>.FromCommand<T>(cmd, commandFrom);
			if (ret.md == null)
			{
				logger.LogError($"Client_OnMessage: {ret.msg}, {Json.Serialize(cmd)}");
				return new MdtResp(false, ret.msg);
			}
			var resp = await mediator.Send(ret.md);
			return resp;
		}

	}
}
