using BazarServer.Entity.Storage;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BazarServer.Application.Posts
{
	public class PostQueryFacade
	{
		/// <summary>
		/// get PostDto based on Post. batch for better performance
		/// </summary>
		/// <param name="postRepository"></param>
		/// <param name="userID"></param>
		/// <param name="ay"></param>
		/// <returns></returns>
		public static async Task<List<PostDto>> GetPostDto(IPostRepository postRepository, string userID, List<Post> ay)
		{
			var postIDs = ay.Select(x => x.postID).ToList();
			var dic = await postRepository.GetPostStatisticAsync(postIDs);
			var dic2 = await postRepository.GetPostLikeAsync(userID, postIDs);

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
				ret.Add(new PostDto(post, dic[post.postID], dic2[post.postID], replyToUser));
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

		public static async Task<List<NotifyDto>> ConvertNotify(IPostRepository postRepository, List<NotifyMessage> ret2)
		{
			List<NotifyDto> ret = new List<NotifyDto>();
			foreach (var noti in ret2)
			{
				if (noti.notifyType == "Like" || noti.notifyType == "Reply" || noti.notifyType == "Mention")
				{
					var postID = noti.fromWhere;
					var post = await postRepository.GetPostAsync(noti.fromWhere);
					if (post == null)
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
					var node = new NotifyDto(noti, post, isReply);
					ret.Add(node);
				}
			}
			return ret;
		}
	}
}
