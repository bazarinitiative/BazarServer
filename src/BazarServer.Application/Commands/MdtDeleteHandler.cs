using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Commands
{
	public class MdtDeleteHandler : IRequestHandler<MdtRequest<DeleteCmd>, MdtResp>
	{
		ILogger<MdtDeleteHandler> _logger;
		IMediator _mediator;

		IGenericMongoCollection<Post> _connPost;
		IGenericMongoCollection<Like> _connLike;
		IGenericMongoCollection<Following> _connFollowing;
		IGenericMongoCollection<Channel> _connChannel;
		IGenericMongoCollection<ChannelMember> _connChannelMember;
		IPostRepository postRepository;
		IUserRepository userRepository;

		public MdtDeleteHandler(IMediator mediator, ILogger<MdtDeleteHandler> logger, IGenericMongoCollection<Post> connPost, IGenericMongoCollection<Like> connLike, IGenericMongoCollection<Following> connFollowing, IGenericMongoCollection<Channel> connChannel, IPostRepository postRepository, IUserRepository userRepository, IGenericMongoCollection<ChannelMember> connChannelMember)
		{
			_mediator = mediator;
			_logger = logger;
			_connPost = connPost;
			_connLike = connLike;
			_connFollowing = connFollowing;
			_connChannel = connChannel;
			this.postRepository = postRepository;
			this.userRepository = userRepository;
			_connChannelMember = connChannelMember;
		}

		public async Task<MdtResp> Handle(MdtRequest<DeleteCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var info = await OnDeleteAsync(req.model, req.commandFrom);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnDeleteAsync(DeleteCmd model, string commandFrom)
		{
			switch (model.deleteType)
			{
				case "Post":
					var oldPost = await _connPost.FirstOrDefaultAsync(x => x.userID == model.userID && x.postID == model.targetID);
					if (oldPost == null)
					{
						return (false, "Target not exist. You can only remove your own object.");
					}
					oldPost.content = "Deleted";
					oldPost.deleted = true;
					oldPost.commandID = model.commandID;
					oldPost.commandTime = model.commandTime;
					await _connPost.UpdateAsync(x => x.postID == oldPost.postID, oldPost);
					await OnPostDelete(oldPost, model, commandFrom);
					break;
				case "Following":
					var oldFollowing = await _connFollowing.FirstOrDefaultAsync(x => x.userID == model.userID && x.targetID == model.targetID);
					if (oldFollowing == null)
					{
						return (false, "Target not exist. You can only remove your own object.");
					}
					await _connFollowing.RemoveAsync(x => x.userID == model.userID && x.targetID == model.targetID);
					if (oldFollowing.targetType == "User")
					{
						await userRepository.UpsertUserStatisticAsync(oldFollowing.userID, 0, 0, -1, 0);
						await userRepository.UpsertUserStatisticAsync(oldFollowing.targetID, 0, 0, 0, -1);
					}
					else
					{
						throw new NotImplementedException();
					}
					break;
				case "Channel":
					var oldChannel = await _connChannel.FirstOrDefaultAsync(x => x.userID == model.userID && x.channelID == model.targetID);
					if (oldChannel == null)
					{
						return (false, "Target not exist. You can only remove your own object.");
					}
					await _connChannelMember.RemoveAsync(x => x.channelID == model.targetID);
					await _connChannel.RemoveAsync(x => x.channelID == model.targetID);
					break;
				case "ChannelMember":
					var oldCM = await _connChannelMember.FirstOrDefaultAsync(x => x.channelID == model.targetID && x.userID == model.userID);
					if (oldCM == null)
					{
						return (false, "Target not exist. You can only maintain your own object.");
					}
					await _connChannelMember.RemoveAsync(x => x.channelID == model.targetID && x.userID == model.userID);
					break;
				case "Like":
					var oldLike = await _connLike.FirstOrDefaultAsync(x => x.userID == model.userID && x.postID == model.targetID);
					if (oldLike == null)
					{
						return (false, "Target not exist. You can only remove your own object.");
					}
					var post = await _connLike.FirstOrDefaultAsync(x => x.postID == oldLike.postID);
					if (post == null)
					{
						return (false, "post not exist");
					}
					await _connLike.RemoveAsync(x => x.userID == model.userID && x.postID == model.targetID);
					await postRepository.UpsertPostStatisticAsync(post.postID, 0, 0, -1);
					await userRepository.UpsertUserStatisticAsync(post.userID, 0, -1, 0, 0);
					await userRepository.RemoveNotify(post.postID);
					break;
				default:
					break;
			}
			return (true, "");
		}

		private async Task OnPostDelete(Post oldPost, DeleteCmd delete, string commandFrom)
		{
			var model = oldPost;
			if (!string.IsNullOrEmpty(model.replyTo))
			{
				if (model.content.Length > 0)
				{
					await postRepository.UpsertPostStatisticAsync(model.replyTo, -1, 0, 0);
				}
				if (model.isRepost)
				{
					await postRepository.UpsertPostStatisticAsync(model.replyTo, 0, -1, 0);
				}
			}

			await userRepository.UpsertUserStatisticAsync(model.userID, 1);

			var meta = await postRepository.GetPostMeta(oldPost.postID);
			if (meta != null)
			{
				meta.receiveDelete = DateTime.Now;
				meta.deleteFrom = commandFrom;
				await postRepository.UpsertPostMeta(meta);
			}

			await userRepository.RemoveNotify(oldPost.postID);
		}
	}
}
