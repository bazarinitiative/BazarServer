using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using BazarServer.Infrastructure.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Commands
{
	public class MdtChannelMemberHandler : IRequestHandler<MdtRequest<ChannelMemberCmd>, MdtResp>
	{
		readonly ILogger<MdtChannelMemberHandler> _logger;
		ISender _mediator;

		IGenericMongoCollection<ChannelMember> _conn;
		IUserRepository userRepository;

		const int maxChannelMemberCount = 200;

		public MdtChannelMemberHandler(ISender mediator, ILogger<MdtChannelMemberHandler> logger, IGenericMongoCollection<ChannelMember> conn, IUserRepository userRepository)
		{
			_mediator = mediator;
			_logger = logger;
			_conn = conn;
			this.userRepository = userRepository;
		}

		public async Task<MdtResp> Handle(MdtRequest<ChannelMemberCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;
				if (!await userRepository.IsExistUserAsync(model.userID))
				{
					return new MdtResp(false, "lack data", new UserCommandRespDto(CommandErrorCode.NoUser, model.userID));
				}
				var info = await OnChannelMemberAsync(model);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnChannelMemberAsync(ChannelMemberCmd cmd)
		{
			var model = new ChannelMember();
			FastCopy.Copy(cmd, model);

			var old = await _conn.FirstOrDefaultAsync(x => x.channelID == model.channelID && x.memberID == model.memberID);
			if (old != null)
			{
				return (false, "dup");
			}
			var count = await _conn.CountAsync(x => x.channelID == model.channelID);
			if (count > maxChannelMemberCount)
			{
				return (false, $"each channel can have {maxChannelMemberCount} members at most");
			}

			await _conn.UpsertAsync(x => x.channelID == model.channelID && x.userID == model.userID, model);

			return (true, "");
		}
	}
}
