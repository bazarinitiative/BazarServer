using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Users
{
	public class MdtUserInfoHandler : IRequestHandler<MdtRequest<UserInfoCmd>, MdtResp>
	{
		ILogger<MdtUserInfoHandler> _logger;
		ISender _mediator;
		IUserRepository userRepository;

		public MdtUserInfoHandler(ISender mediator, ILogger<MdtUserInfoHandler> logger, IUserRepository userRepository)
		{
			_mediator = mediator;
			_logger = logger;
			this.userRepository = userRepository;
		}

		public async Task<MdtResp> Handle(MdtRequest<UserInfoCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;
				if (model.userID != Encryption.CalculateUserID(model.publicKey))
				{
					return new MdtResp(false, "userID not match publicKey");
				}

				if (!model.userID.IsLetterDigit30())
				{
					return new MdtResp(false, "invalid userID");
				}
				if (model.biography.Length > 300)
				{
					return new MdtResp(false, "biography too long");
				}
				if (model.location.Length > 100)
				{
					return new MdtResp(false, "location too long");
				}
				if (Json.Serialize(model).Length > 1000)
				{
					return new MdtResp(false, "user model too big");
				}

				var info = await OnUserInfoAsync(model);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnUserInfoAsync(UserInfoCmd cmd)
		{
			UserInfo model = new UserInfo();
			FastCopy.Copy(cmd, model);

			var old = await userRepository.GetUserInfoAsync(model.userID);
			if (old != null)
			{
				if (model.publicKey != old.publicKey)
				{
					return (false, "cannot update publicKey");
				}
				if (model.commandTime < old.commandTime)
				{
					return (false, "older can not update newer");
				}
				model.createTime = old.createTime;
				if (model.createTime == 0)
				{
					model.createTime = old.commandTime;
				}

				await userRepository.SaveUserAsync(model);
			}
			else
			{
				model.createTime = DateHelper.CurrentTimeMillis();
				await userRepository.SaveUserAsync(model);
			}
			return (true, "");
		}

	}
}
