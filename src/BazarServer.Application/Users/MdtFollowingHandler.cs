using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Users
{
	public class MdtFollowingHandler : IRequestHandler<MdtRequest<FollowingCmd>, MdtResp>
	{
		readonly ILogger<MdtFollowingHandler> _logger;
		ISender _mediator;

		IGenericMongoCollection<Following> _conn;
		IUserRepository userRepository;
		IGenericMongoCollection<Channel> _connChannel;

		public MdtFollowingHandler(ISender mediator, ILogger<MdtFollowingHandler> logger, IGenericMongoCollection<Following> conn, IUserRepository userRepository, IGenericMongoCollection<Channel> connChannel)
		{
			_mediator = mediator;
			_logger = logger;
			_conn = conn;
			this.userRepository = userRepository;
			_connChannel = connChannel;
		}

		public async Task<MdtResp> Handle(MdtRequest<FollowingCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;
				if (!model.targetID.IsLetterDigit30())
				{
					return new MdtResp(false, $"invalid tagetID[{model.targetID}], length[{model.targetID.Length}]");
				}
				if (model.targetID == model.userID)
				{
					return new MdtResp(false, $"can't follow yourself");
				}
				UserCommandRespDto rc = new UserCommandRespDto();
				if (!await userRepository.IsExistUserAsync(model.userID))
				{
					rc.AddUser(model.userID);
					return new MdtResp(false, "lack data", rc);
				}
				if (model.targetType == "User")
				{
					if (!await userRepository.IsExistUserAsync(model.targetID))
					{
						rc.AddUser(model.targetID);
						return new MdtResp(false, "lack data", rc);
					}
				}
				else if (model.targetType == "Channel")
				{
					var cc = await _connChannel.FirstOrDefaultAsync(x => x.channelID == model.targetID);
					if (cc == null)
					{
						rc.AddChannel(model.targetID);
						return new MdtResp(false, "lack data", rc);
					}
				}
				else
				{
					return new MdtResp(false, $"unkow targetType: {model.targetType}");
				}
				var info = await OnFollowingAsync(model);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnFollowingAsync(FollowingCmd cmd)
		{
			var model = new Following();
			FastCopy.Copy(cmd, model);

			var old = await _conn.FirstOrDefaultAsync(x => x.userID == model.userID && x.targetID == model.targetID);
			if (old != null)
			{
				return (false, "dup");
			}
			else
			{
				await _conn.UpsertAsync(x => x.userID == model.userID && x.targetID == model.targetID, model);

				if (model.targetType == "User")
				{
					await userRepository.UpsertUserStatisticAsync(model.userID, 0, 0, 1, 0);
					await userRepository.UpsertUserStatisticAsync(model.targetID, 0, 0, 0, 1);
				}
			}
			return (true, "");
		}
	}
}
