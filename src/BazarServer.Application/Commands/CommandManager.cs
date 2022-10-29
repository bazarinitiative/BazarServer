using BazarServer.Application.Common;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using System;

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

			if (string.IsNullOrEmpty(cmd.version))
			{
				//v0.1
				cmd.version = "v0.1";
			}
			if (!Decimal.TryParse(cmd.version.Substring(1), out decimal version))
			{
				return new MdtResp(false, $"invalid version ");
			}
			DateTime dtv2start = new DateTime(2022, 10, 29);
			DateTime dtv2enforce = new DateTime(2022, 11, 30);
			DateTime commandTime = DateHelper.FromTimestamp(cmd.commandTime, TimeZoneInfo.Utc);
			if (version >= 0.2m && commandTime < dtv2start)
			{
				return new MdtResp(false, $"version and time not match: {version} {commandTime}");
			}
			if (version == 0.1m && commandTime > dtv2enforce)
			{
				return new MdtResp(false, $"version and time not match: {version} {commandTime}");
			}

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
						resp = await ProcessCommand<UserInfoCmd>(cmd, commandFrom);
						break;
					case "UserPic":
						resp = await ProcessCommand<UserPicCmd>(cmd, commandFrom);
						break;
					case "Following":
						resp = await ProcessCommand<FollowingCmd>(cmd, commandFrom);
						break;
					case "Channel":
						resp = await ProcessCommand<ChannelCmd>(cmd, commandFrom);
						break;
					case "ChannelMember":
						resp = await ProcessCommand<ChannelMemberCmd>(cmd, commandFrom);
						break;
					case "Post":
						resp = await ProcessCommand<PostCmd>(cmd, commandFrom);
						break;
					case "Like":
						resp = await ProcessCommand<LikeCmd>(cmd, commandFrom);
						break;
					case "Bookmark":
						resp = await ProcessCommand<BookmarkCmd>(cmd, commandFrom);
						break;
					case "Delete":
						resp = await ProcessCommand<DeleteCmd>(cmd, commandFrom);
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

		private async Task<MdtResp> ProcessCommand<T>(UserCommand cmd, string commandFrom) where T : ICommandContent
		{
			var creq = MdtRequest<T>.FromCommand<T>(cmd, commandFrom);
			if (creq.md == null)
			{
				logger.LogError($"Client_OnMessage: {creq.msg}, {Json.Serialize(cmd)}");
				return new MdtResp(false, creq.msg);
			}
			if (string.IsNullOrEmpty(creq.md.model.commandType))
			{
				if (cmd.version == "v0.1")
				{
					creq.md.model.commandType = cmd.commandType;
				}
			}
			if (creq.md.model.commandType != cmd.commandType)
			{
				return new MdtResp(false, $"commandType not match");
			}
			var resp = await mediator.Send(creq.md);
			return resp;
		}

	}
}
