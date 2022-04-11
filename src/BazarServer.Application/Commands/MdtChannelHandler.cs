using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using BazarServer.Infrastructure.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Commands
{
	public class MdtChannelHandler : IRequestHandler<MdtRequest<ChannelCmd>, MdtResp>
	{
		readonly ILogger<MdtChannelHandler> _logger;
		ISender _mediator;

		IGenericMongoCollection<Channel> _conn;
		IUserRepository userRepository;

		const int maxChannelCount = 50;

		public MdtChannelHandler(ISender mediator, ILogger<MdtChannelHandler> logger, IGenericMongoCollection<Channel> conn, IUserRepository userRepository)
		{
			_mediator = mediator;
			_logger = logger;
			_conn = conn;
			this.userRepository = userRepository;
		}

		public async Task<MdtResp> Handle(MdtRequest<ChannelCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;
				if (model.channelName.Length > 50)
				{
					return new MdtResp(false, "channelName too long");
				}
				if (model.description.Length > 300)
				{
					return new MdtResp(false, "description too long");
				}
				if (!await userRepository.IsExistUserAsync(model.userID))
				{
					return new MdtResp(false, "lack data", new UserCommandRespDto(CommandErrorCode.NoUser, model.userID));
				}
				var old = await _conn.FirstOrDefaultAsync(x => x.userID == model.userID && x.channelName == model.channelName);
				if (old != null)
				{
					return new MdtResp(false, "duplicate channelName already exist");
				}
				var count = await _conn.CountAsync(x => x.userID == model.userID);
				if (count > maxChannelCount)
				{
					return new MdtResp(false, $"you can only have {maxChannelCount} channels at most");
				}
				var info = await OnChannelAsync(model);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnChannelAsync(ChannelCmd cmd)
		{
			Channel model = new Channel(cmd.channelID, cmd.userID, cmd.channelName);
			FastCopy.Copy(cmd, model);

			await _conn.UpsertAsync(x => x.channelID == model.channelID, model);

			return (true, "");
		}
	}
}
