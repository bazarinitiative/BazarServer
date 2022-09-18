using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using Common.Utils;
using System.Collections.Concurrent;

namespace BazarServer.Application.Query
{
	public class PostQueryFacade
	{
		/// <summary>
		/// get PostDto based on Post. batch for better performance
		/// </summary>
		/// <param name="postRepository"></param>
		/// <param name="userID">from whose point of view</param>
		/// <param name="ay"></param>
		/// <returns></returns>
		public static async Task<List<PostDto>> GetPostDto(IPostRepository postRepository, string userID, List<Post> ay)
		{
			var postIDs = ay.Select(x => x.postID).ToList();
			var dic = await postRepository.GetPostStatisticAsync(postIDs);
			var dic2 = await postRepository.GetPostLikeAsync(userID, postIDs);
			var dic3 = await postRepository.GetPostBookmarkAsync(userID, postIDs);

			var replyTos = ay.Select(x => x.replyTo).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
			var dicReplyTos = await postRepository.GetPostsAsync(replyTos);

			List<PostDto> ret = new List<PostDto>();
			foreach (var post in ay)
			{
				var replyToUser = "";
				if (dicReplyTos.ContainsKey(post.replyTo))
				{
					replyToUser = dicReplyTos[post.replyTo].userID;
				}
				ret.Add(new PostDto(post, dic[post.postID], dic2[post.postID], replyToUser, dic3[post.postID]));
			}
			return ret;
		}

		static string GetUserLatestPostsKey(string userID)
		{
			return $"GetUserLatestPosts_{userID}";
		}

		/// <summary>
		/// get latest N posts of user
		/// </summary>
		/// <param name="postRepository"></param>
		/// <param name="userID"></param>
		/// <returns></returns>
		private static async Task<List<Post>> GetUserLatestPosts_withCache(IPostRepository postRepository, string userID)
		{
			// work with RemoveUserLatestPostsCache
			var cacheMilli = 7 * 24 * 3600 * 1000;
			var key = GetUserLatestPostsKey(userID);
			var list = await CacheHelper.WithCacheAsync<List<Post>>(key, async () =>
			{
				var ret = await postRepository.GetPostsByUserAsync(userID, false, 0, 100);
				return ret;
			}, cacheMilli, true);
			if (list == null)
			{
				list = new List<Post>();
			}
			return list;
		}

		/// <summary>
		/// remove cache when mongodb has 'Post' change event
		/// </summary>
		/// <param name="userID"></param>
		public static void RemoveUserLatestPostsCache(string userID)
		{
			var key = GetUserLatestPostsKey(userID);
			CacheHelper.RemoveCache(key);
		}

		/// <summary>
		/// return the top N posts of users. latest at top. with cache.
		/// </summary>
		/// <param name="postRepository"></param>
		/// <param name="userIDs"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public static async Task<List<Post>> GetLatestPostsByUsers(IPostRepository postRepository, List<string> userIDs, int page, int pageSize)
		{
			List<Post> posts = new List<Post>();
			foreach (var userID in userIDs)
			{
				var sub = await GetUserLatestPosts_withCache(postRepository, userID);
				posts.AddRange(sub);
			}
			var ret = posts.OrderByDescending(x => x.commandTime).Take(new Range(page * pageSize, (page + 1) * pageSize)).ToList();
			return ret;
		}

		public static async Task<List<NotifyDto>> ConvertNotify(IPostRepository postRepository, string userID, List<NotifyMessage> ret2)
		{
			var postIDs = ret2.Select(x => x.fromWhere).ToList();
			var dicPost = await postRepository.GetPostsAsync(postIDs);
			var ayDto = await PostQueryFacade.GetPostDto(postRepository, userID, dicPost.Values.ToList());
			var dicDto = ayDto.ToDictionary(x => x.post.postID);

			List<NotifyDto> ret = new List<NotifyDto>();
			foreach (var noti in ret2)
			{
				if (noti.notifyType == "Like" || noti.notifyType == "Reply" || noti.notifyType == "Mention")
				{
					var postID = noti.fromWhere;
					dicDto.TryGetValue(noti.fromWhere, out var dto);
					if (dto == null)
					{
						continue;
					}
					foreach (var item in ret)
					{
						if (item.noti.fromWhere == noti.fromWhere && item.noti.fromWho == noti.fromWho)
						{
							//ignore dup
							continue;
						}
					}
					bool isReply = (noti.notifyType == "Reply");
					var node = new NotifyDto(noti, dto, isReply);
					ret.Add(node);
				}
			}
			return ret;
		}

		static ConcurrentDictionary<string, Post> dicPosts = new ConcurrentDictionary<string, Post>();

		/// <summary>
		/// get random posts with reasonable cache
		/// </summary>
		/// <param name="postRepository"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static async Task<List<Post>> GetRandomPostsWithCache(IPostRepository postRepository, int count)
		{
			while (dicPosts.Count < count * 2)
			{
				await CacheSomePosts(postRepository);
			}

			_ = Task.Run(() => MaintainCache(postRepository));

			var ay = MyRandom.RandomList(0, dicPosts.Count, count);
			List<Post> res = new List<Post>();
			foreach (var idx in ay)
			{
				res.Add(dicPosts.ElementAt(idx).Value);
			}
			return res;
		}

		static DateTime lastMaintain = DateTime.Now;

		private static async Task MaintainCache(IPostRepository postRepository)
		{
			var span = lastMaintain - DateTime.Now;
			if (span.TotalSeconds < 60)
			{
				return;
			}
			lastMaintain = DateTime.Now;

			await CacheSomePosts(postRepository);
			if (dicPosts.Count > 10000)
			{
				var ay = dicPosts.Values.OrderByDescending(x => x.commandTime).Take(1000);
				foreach (var item in ay)
				{
					dicPosts.TryRemove(item.postID, out _);
				}
			}
		}

		private static async Task CacheSomePosts(IPostRepository postRepository)
		{
			var posts = (await postRepository.GetRandomPost(100)).OrderByDescending(x => x.commandTime).Take(50);
			foreach (var item in posts)
			{
				dicPosts.TryAdd(item.postID, item);
			}
		}
	}
}
