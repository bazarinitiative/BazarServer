using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Storage;
using Common.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BazarServer.Infrastructure.Repository
{
	public class PostRepository : IPostRepository
	{
		IGenericMongoCollection<Post> _conn;
		IGenericMongoCollection<Like> _connLike;
		IGenericMongoCollection<PostStatistic> _connStat;
		IGenericMongoCollection<PostMeta> _connMeta;
		IGenericMongoCollection<Bookmark> _connBookmark;
		ILogger<PostRepository> _logger;

		public PostRepository(IGenericMongoCollection<Post> conn, IGenericMongoCollection<PostStatistic> connStat, ILogger<PostRepository> logger, IGenericMongoCollection<Like> connLike, IGenericMongoCollection<PostMeta> connMeta, IGenericMongoCollection<Bookmark> connBookmark)
		{
			_conn = conn;
			_connStat = connStat;
			_logger = logger;
			_connLike = connLike;
			_connMeta = connMeta;
			_connBookmark = connBookmark;
		}

		public async Task<List<Post>> GetPostsByUserAsync(string userID, bool onlyOriginalPost, int page, int pageSize)
		{
			if (onlyOriginalPost)
			{
				var ay = await _conn.PageAsync(x => x.userID == userID && !x.deleted && x.replyTo == "",
								   x => x.commandTime,
								   page,
								   pageSize,
								   true);
				return ay;
			}
			else
			{
				var ay = await _conn.PageAsync(x => x.userID == userID && !x.deleted,
								   x => x.commandTime,
								   page,
								   pageSize,
								   true);
				return ay;
			}
		}

		public async Task<Post?> GetPostAsync(string postID)
		{
			var model = await _conn.FirstOrDefaultAsync(x => x.postID == postID);
			return model;
		}

		public async Task<Dictionary<string, PostStatistic>> GetPostStatisticAsync(List<string> postIDs)
		{
			var list = await _connStat.InAsync(nameof(Post.postID), postIDs);
			var dic = list.ToDictionary(x => x.postID);

			postIDs.ForEach(x =>
			{
				if (!dic.ContainsKey(x))
				{
					dic.Add(x, new PostStatistic(x));
				}
			});
			return dic;
		}

		/// <summary>
		/// get latest posts
		/// </summary>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <param name="lang"></param>
		/// <returns></returns>
		public async Task<List<Post>> TimelineAsync(int page, int pageSize, string? lang)
		{
			var qry = _conn.GetQueryable().Where(x => !x.deleted);
			if (!string.IsNullOrEmpty(lang))
			{
				qry = qry.Where(x => x.contentLang == lang);
			}
			var ay = await qry.OrderByDescending(x => x.commandTime)
						.Skip(page * pageSize)
						.Take(pageSize)
						.ToListAsync();
			return ay;
		}

		public async Task SaveAsync(Post model)
		{
			await _conn.UpsertAsync(x => x.postID == model.postID, model);
		}

		public async Task UpsertPostStatisticAsync(string postID, int addReplyCount, int addRepostCount, int addLikeCount)
		{
			try
			{
				var ret = await _connStat.FirstOrDefaultAsync(x => x.postID == postID);
				if (ret == null)
				{
					ret = new PostStatistic(postID);
				}
				ret.replyCount += addReplyCount;
				ret.repostCount += addRepostCount;
				ret.likeCount += addLikeCount;
				await _connStat.UpsertAsync(x => x.postID == postID, ret);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"post:{postID}");
			}
		}

		public async Task UpsertPostMeta(PostMeta model)
		{
			await _connMeta.UpsertAsync(x => x.postID == model.postID, model);
		}

		public async Task<PostMeta?> GetPostMeta(string postID)
		{
			var ret = await _connMeta.FirstOrDefaultAsync(x => x.postID == postID);
			return ret;
		}

		public async Task<bool> IsExistPostAsync(string postID)
		{
			var ret = await GetPostAsync(postID);
			return (ret != null);
		}

		public async Task<Dictionary<string, bool>> GetPostLikeAsync(string userID, List<string> postIDs)
		{
			var list = await _connLike.InFilterAsync(nameof(Post.postID), postIDs, x => x.userID == userID);
			var set = list.Select(x => x.postID).ToHashSet();

			Dictionary<string, bool> ret = new Dictionary<string, bool>();
			foreach (var postID in postIDs)
			{
				ret[postID] = set.Contains(postID);
			}
			return ret;
		}

		public async Task<Dictionary<string, bool>> GetPostBookmarkAsync(string userID, List<string> postIDs)
		{
			var list = await _connBookmark.InFilterAsync(nameof(Post.postID), postIDs, x => x.userID == userID);
			var set = list.Select(x => x.postID).ToHashSet();

			Dictionary<string, bool> ret = new Dictionary<string, bool>();
			foreach (var postID in postIDs)
			{
				ret[postID] = set.Contains(postID);
			}
			return ret;
		}

		public async Task<PostStatistic?> GetPostStatisticAsync(string postID)
		{
			var stat = await _connStat.FirstOrDefaultAsync(x => x.postID == postID);
			return stat;
		}

		public async Task<bool> GetPostLikeAsync(string userID, string postID)
		{
			var like = await _connLike.FirstOrDefaultAsync(x => x.userID == userID && x.postID == postID);
			if (like != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public async Task<bool> GetPostBookmarkAsync(string userID, string postID)
		{
			var bookmark = await _connBookmark.FirstOrDefaultAsync(x => x.userID == userID && x.postID == postID);
			if (bookmark != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public async Task<List<string>> GetRepliesAsync(string postID, int page, int pageSize)
		{
			var list = await _conn.PageAsync(x => x.replyTo == postID && x.content.Length > 0 && x.deleted == false,
												x => x.commandTime,
												page,
												pageSize,
												true);
			var ret = list.Select(x => x.postID).ToList();
			return ret;
		}

		public async Task<Dictionary<string, Post>> GetPostsAsync(List<string> postIDs)
		{
			var list = await _conn.InAsync(nameof(Post.postID), postIDs);
			return list.ToDictionary(x => x.postID);
		}

		public async Task<List<Post>> Search(List<string> ay, int startIdx, int endIdx)
		{
			var ret = await _conn.Search(ay, startIdx, endIdx, x => x.commandTime);
			return ret;
		}

		public async Task<List<Post>> GetRandomPost(int count)
		{
			var ret = await _conn.Random(count);
			return ret;
		}

		public async Task UpsertBookmark(Bookmark model)
		{
			await _connBookmark.UpsertAsync(x => x.userID == model.userID && x.postID == model.postID, model);
		}

		public async Task<List<Bookmark>> GetUserBookmarks(string userID, int page, int pageSize)
		{
			var ret = await _connBookmark.PageAsync(x => x.userID == userID, x => x.commandTime, page, pageSize, true);
			return ret;
		}

		public async Task<List<Post>> GetPostNoLang(long lastPostTime, int count)
		{
			var ret = await _conn.PageAsync(x => x.commandTime >= lastPostTime && x.contentLang == "",
									x => x.commandTime, 0, count, false);
			return ret;
		}
	}
}
