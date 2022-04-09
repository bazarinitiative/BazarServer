using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BazarServer.Application.Commands
{
	public class MdtUserPicHandler : IRequestHandler<MdtRequest<UserPicCmd>, MdtResp>
	{
		ILogger<MdtUserPicHandler> _logger;
		ISender _mediator;
		IGenericMongoCollection<UserPic> _conn;

		public MdtUserPicHandler(ISender mediator, ILogger<MdtUserPicHandler> logger, IGenericMongoCollection<UserPic> conn)
		{
			_mediator = mediator;
			_logger = logger;
			_conn = conn;
		}

		public async Task<MdtResp> Handle(MdtRequest<UserPicCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;
				var info = await OnUserPicAsync(model);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnUserPicAsync(UserPicCmd cmd)
		{
			UserPic model = new UserPic(cmd.userID);
			FastCopy.Copy(cmd, model);

			if (model.pic.Length > 50 * 1024)
			{
				return (false, "data too long");
			}

			var bs = Convert.FromBase64String(model.pic);
			using var ms = new MemoryStream(bs);
			using var img = await Image.LoadAsync(ms);
			if (img.Width != img.Height)
			{
				return (false, "userPic should be square");
			}

			var old = await _conn.FirstOrDefaultAsync(x => x.userID == model.userID);
			if (old != null)
			{
				if (model.commandTime < old.commandTime)
				{
					return (false, "older can not update newer");
				}

				old.commandID = model.commandID;
				old.commandTime = model.commandTime;
				old.pic = model.pic;
				await _conn.UpdateAsync(x => x.userID == model.userID, old);
			}
			else
			{
				await _conn.UpsertAsync(x => x.userID == model.userID, model);
			}
			return (true, "");
		}
	}
}
