using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure;
using BazarServer.Infrastructure.Repository;
using BazarServer.Infrastructure.Storage;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BazarServer.Application.Commands
{
	public class MdtBookmarkHandler : IRequestHandler<MdtRequest<BookmarkCmd>, MdtResp>
	{
		ILogger<MdtBookmarkHandler> _logger;
		ISender _mediator;

		IGenericMongoCollection<Bookmark> _conn;
		IPostRepository postRepository;
		IUserRepository userRepository;

		public MdtBookmarkHandler(ISender mediator, ILogger<MdtBookmarkHandler> logger, IGenericMongoCollection<Bookmark> conn, IPostRepository postRepository, IUserRepository userRepository)
		{
			_mediator = mediator;
			_logger = logger;
			_conn = conn;
			this.postRepository = postRepository;
			this.userRepository = userRepository;
		}

		public async Task<MdtResp> Handle(MdtRequest<BookmarkCmd> req, CancellationToken cancellationToken)
		{
			try
			{
				var model = req.model;
				var post = await postRepository.GetPostAsync(model.postID);
				if (post == null)
				{
					return new MdtResp(false, "lack data", new UserCommandRespDto(CommandErrorCode.NoPost, model.postID));
				}
				var info = await OnBookmarkAsync(model, post);

				return new MdtResp(info);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{req}");
				return new MdtResp(false, ex.Message);
			}
		}

		private async Task<(bool success, string msg)> OnBookmarkAsync(BookmarkCmd cmd, Post post)
		{
			Bookmark model = new Bookmark(cmd.postID);
			FastCopy.Copy(cmd, model);

			var old = await _conn.FirstOrDefaultAsync(x => x.userID == model.userID && x.postID == model.postID);
			if (old != null)
			{
				return (false, "duplicate bookmark already exist");
			}
			else
			{
				await _conn.UpsertAsync(x => x.userID == model.userID && x.postID == model.postID, model);
			}

			return (true, "");
		}
	}
}
