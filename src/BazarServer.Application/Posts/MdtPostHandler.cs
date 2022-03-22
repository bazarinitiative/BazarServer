using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Posts
{
	public class MdtPostHandler : IRequestHandler<MdtRequest<PostCmd>, MdtResp>
	{
		ILogger<MdtPostHandler> _logger;
		IMediator _mediator;
		IPostRepository postRepository;
		IUserRepository userRepository;

		public MdtPostHandler(IMediator mediator, ILogger<MdtPostHandler> logger, IPostRepository postRepository, IUserRepository userRepository)
		{
			_mediator = mediator;
			_logger = logger;
			this.postRepository = postRepository;
			this.userRepository = userRepository;
		}

		public async Task<MdtResp> Handle(MdtRequest<PostCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;

				if (!model.postID.IsLetterDigit30())
				{
					return new MdtResp(false, "invalid postID");
				}
				if (!model.userID.IsLetterDigit30())
				{
					return new MdtResp(false, "invalid auther");
				}
				if (!model.threadID.IsLetterDigit30())
				{
					return new MdtResp(false, "every post need a valid threadID");
				}
				if (model.replyTo.Length != 0 && model.replyTo.Length != 30)
				{
					return new MdtResp(false, "invalid replyID");
				}
				if (model.content.Length > 300)
				{
					return new MdtResp(false, "content too long");
				}
				var poststr = Json.Serialize(model);
				if (poststr.Length > 1000)
				{
					return new MdtResp(false, "post model too big");
				}

				if (model.replyTo.Length > 0)
				{
					var parent = await postRepository.GetPostAsync(model.replyTo);
					if (parent == null)
					{
						return new MdtResp(false, "lack data", new UserCommandRespDto(CommandErrorCode.NoPost, model.replyTo));
					}
					if (model.threadID != parent.threadID)
					{
						return new MdtResp(false, "threadID not match");
					}
				}

				var old = await postRepository.GetPostAsync(model.postID);
				if (old != null)
				{
					return new MdtResp(false, $"dup {old.postID}");
				}

				var info = await OnPostAsync(model, req.commandFrom);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnPostAsync(PostCmd cmd, string commandFrom)
		{
			Post model = new Post();
			FastCopy.Copy(cmd, model);

			await postRepository.SaveAsync(model);


			var meta = new PostMeta()
			{
				postID = model.postID,
				receiveCreate = DateTime.Now,
				createFrom = commandFrom,
				inheritPosts = ""
			};
			PostMeta? parentMeta = null;
			if (cmd.replyTo.Length > 0)
			{
				parentMeta = await postRepository.GetPostMeta(cmd.replyTo);
				if (parentMeta == null || parentMeta.inheritPosts == "")
				{
					meta.inheritPosts = cmd.replyTo;
				}
				else
				{
					meta.inheritPosts = $"{parentMeta.inheritPosts},{cmd.replyTo}";
					meta.inheritPosts = meta.inheritPosts.Trim(',');
				}
			}
			await postRepository.UpsertPostMeta(meta);

			if (meta.inheritPosts.Length > 0)
			{
				var ayPostID = meta.inheritPosts.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
				var ayPost = await postRepository.GetPostsAsync(ayPostID);
				foreach (var post in ayPost.Values)
				{
					var noti = new NotifyMessage()
					{
						notifyID = MyRandom.RandomString(30),
						notifyTime = cmd.commandTime,
						notifyType = "Mention",
						userID = post.userID,
						fromWho = cmd.userID,
						fromWhere = cmd.postID
					};

					//fire and forget. currently we don't care if this noti lost. may be better later.
					_ = userRepository.AddUserNotify(noti);
				}
			}

			if (!string.IsNullOrEmpty(model.replyTo))
			{
				if (model.content.Length > 0)
				{
					await postRepository.UpsertPostStatisticAsync(model.replyTo, 1, 0, 0);
				}
				if (model.isRepost)
				{
					await postRepository.UpsertPostStatisticAsync(model.replyTo, 0, 1, 0);
				}

			}
			await userRepository.UpsertUserStatisticAsync(model.userID, 1);

			return (true, "");
		}

	}
}
