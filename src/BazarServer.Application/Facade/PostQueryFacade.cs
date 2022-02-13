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

			List<PostDto> ret = new List<PostDto>();
			foreach (var post in ay)
			{
				ret.Add(new PostDto(post, dic[post.postID], dic2[post.postID]));
			}
			return ret;
		}

		static string GetUserLatestPostsKey(string userID)
		{
			return $"GetUserLatestPosts_{userID}";
		}

		/// <summary>
		/// get latest 1000 posts of user
		/// </summary>
		/// <param name="postRepository"></param>
		/// <param name="userID"></param>
		/// <returns></returns>
		private static async Task<List<Post>> GetUserLatestPosts_withCache(IPostRepository postRepository, string userID)
		{
			var cacheMilli = 60 * 1000 + MyRandom.Random(0, 1000);
			var key = GetUserLatestPostsKey(userID);
			var list = await CacheHelper.WithCacheAsync<List<Post>>(key, async () => {
				var ret = await postRepository.GetPostsByUserAsync(userID, false, 0, 1000);
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
		/// return the posts of users. latest at top.
		/// </summary>
		/// <param name="postRepository"></param>
		/// <param name="userIDs"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public static async Task<List<Post>> GetPostsByUsers(IPostRepository postRepository, List<string> userIDs, int page, int pageSize)
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
	}
}
