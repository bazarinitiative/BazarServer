using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Posts
{
	public class MdtLikeHandler : IRequestHandler<MdtRequest<LikeCmd>, MdtResp>
	{
		ILogger<MdtLikeHandler> _logger;
		ISender _mediator;

		IGenericMongoCollection<Like> _conn;
		IPostRepository postRepository;
		IUserRepository userRepository;

		public MdtLikeHandler(ISender mediator, ILogger<MdtLikeHandler> logger, IGenericMongoCollection<Like> conn, IPostRepository postRepository, IUserRepository userRepository)
		{
			_mediator = mediator;
			_logger = logger;
			_conn = conn;
			this.postRepository = postRepository;
			this.userRepository = userRepository;
		}

		public async Task<MdtResp> Handle(MdtRequest<LikeCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;
				var post = await postRepository.GetPostAsync(model.postID);
				if (post == null)
				{
					return new MdtResp(false, "lack data", new UserCommandRespDto(CommandErrorCode.NoPost, model.postID));
				}
				var info = await OnLikeAsync(model, post);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnLikeAsync(LikeCmd cmd, Post post)
		{
			Like model = new Like();
			FastCopy.Copy(cmd, model);

			var old = await _conn.FirstOrDefaultAsync(x => x.userID == model.userID && x.postID == model.postID);
			if (old != null)
			{
				return (false, "dup");
			}
			else
			{
				await _conn.UpsertAsync(x => x.userID == model.userID && x.postID == model.postID, model);

				await postRepository.UpsertPostStatisticAsync(model.postID, 0, 0, 1);
				await userRepository.UpsertUserStatisticAsync(post.userID, 0, 1, 0, 0);
			}

			NotifyMessage noti = new NotifyMessage()
			{
				userID = post.userID,
				notifyTime = cmd.commandTime,
				notifyType = "Like",
				notifyID = MyRandom.RandomString(30),
				fromWho = cmd.userID,
				fromWhere = cmd.postID
			};
			await userRepository.AddUserNotify(noti);

			return (true, "");
		}
	}
}
