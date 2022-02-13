using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Users
{
	public class MdtChannelMemberHandler : IRequestHandler<MdtRequest<ChannelMemberCmd>, MdtResp>
	{
		readonly ILogger<MdtChannelMemberHandler> _logger;
		ISender _mediator;

		IGenericMongoCollection<ChannelMember> _conn;
		IUserRepository userRepository;

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
				UserCommandRespDto rc = new UserCommandRespDto();
				if (!await userRepository.IsExistUserAsync(model.userID))
				{
					rc.AddUser(model.userID);
					return new MdtResp(false, "lack data", rc);
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

			var old = await _conn.FirstOrDefaultAsync(x => x.channelID == model.channelID && x.userID == model.userID);
			if (old != null)
			{
				return (false, "dup");
			}

			await _conn.UpsertAsync(x => x.channelID == model.channelID && x.userID == model.userID, model);

			return (true, "");
		}
	}
}
