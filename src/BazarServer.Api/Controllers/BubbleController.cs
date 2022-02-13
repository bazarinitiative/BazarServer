using BazarServer.Application.Posts;
using BazarServer.Application.Users;
using BazarServer.Entity.SeedWork;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Users;
using MediatR;

namespace BazarServer.Controllers;

/// <summary>
/// for those search and recommendation
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class BubbleController : BazarControllerBase
{
	IUserRepository userRepository;
	IPostRepository postRepository;

	public BubbleController(IUserRepository userRepository, IPostRepository postRepository)
	{
		this.userRepository = userRepository;
		this.postRepository = postRepository;
	}

	/// <summary>
	/// return personal search result
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="queryTime"></param>
	/// <param name="token"></param>
	/// <param name="keys"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<SearchResult>> Search(string userID, long queryTime, string token, string keys)
	{
		var ret = new SearchResult();
		var check = await UserQueryFacade.CheckQuery(userRepository, userID, queryTime, token);
		if (!check.success)
		{
			return Error(check.msg, ret);
		}
		var ay = keys.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
		var users = await userRepository.Search(ay, 10);
		foreach (var user in users)
		{
			UserDto dd = await UserQueryFacade.GetUserDto_WithCache(userRepository, user.userID);
			ret.users.Add(dd);
		}
		var posts = await postRepository.Search(ay, 10);
		foreach (var post in posts)
		{
			PostStatistic ps = await postRepository.GetPostStatisticAsync(post.postID) ?? new PostStatistic(post.postID);
			var liked = await postRepository.GetPostLikeAsync(userID, post.postID);
			PostDto dd = new PostDto(post, ps, liked);
			ret.posts.Add(dd);
		}
		return Success(ret);
	}

	/// <summary>
	/// public search
	/// </summary>
	/// <param name="keys"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<SearchResult>> PublicSearch(string keys)
	{
		var ret = new SearchResult();
		var ay = keys.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
		var users = await userRepository.Search(ay, 10);
		foreach (var user in users)
		{
			UserDto dd = await UserQueryFacade.GetUserDto_WithCache(userRepository, user.userID);
			ret.users.Add(dd);
		}
		var posts = await postRepository.Search(ay, 10);
		foreach (var post in posts)
		{
			PostStatistic ps = await postRepository.GetPostStatisticAsync(post.postID) ?? new PostStatistic(post.postID);
			var liked = false;
			PostDto dd = new PostDto(post, ps, liked);
			ret.posts.Add(dd);
		}
		return Success(ret);
	}

	public class SearchResult
	{
		public List<UserDto> users { get; set; } = new List<UserDto>();
		public List<PostDto> posts { get; set; } = new List<PostDto>();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="queryTime">timeMillis</param>
	/// <param name="token"></param>
	/// <param name="count">max 100</param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<UserDto>>> MightLike(string userID, long queryTime, string token, int count = 3)
	{
		if (count > 100)
		{
			count = 100;
		}

		List<UserDto> ret = new List<UserDto>();
		var check = await UserQueryFacade.CheckQuery(userRepository, userID, queryTime, token);
		if (!check.success)
		{
			return Error(check.msg, ret);
		}

		var ay = await userRepository.GetRandomUser(count);
		foreach (var user in ay)
		{
			UserDto dto = await UserQueryFacade.GetUserDto_WithCache(userRepository, user.userID);
			ret.Add(dto);
		}
		return Success(ret);
	}

	public class TrendUnit
	{
		/// <summary>
		/// the key word of this trend. usually for search
		/// </summary>
		public string key { get; set; } = "";
		/// <summary>
		/// catalog of this trending. Politics, Sports, Gaming, etc. or empty.
		/// </summary>
		public string catalog { get; set; } = "";
		/// <summary>
		/// describe why trending. "1152 posts", "from recommend engine", "from partner" etc.
		/// </summary>
		public string describe { get; set; } = "";
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="queryTime"></param>
	/// <param name="token"></param>
	/// <param name="count">max 100</param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<TrendUnit>>> Trending(string userID, long queryTime, string token, int count = 5)
	{
		if (count > 100)
		{
			count = 100;
		}
		var users = await userRepository.GetRandomUser(100);
		var posts = await postRepository.GetRandomPost(100);
		List<string> pool = new List<string>();
		foreach (var user in users)
		{
			pool.Add(user.userName);
		}
		foreach (var post in posts)
		{
			var sub = post.content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			pool.AddRange(sub);
		}

		List<TrendUnit> ret = new List<TrendUnit>();
		for (int i = 0; i < count; i++)
		{
			int idx = MyRandom.Random(0, pool.Count);
			TrendUnit item = new TrendUnit()
			{
				key = pool[idx],
				catalog = "",
				describe = "from xEngine"
			};
			ret.Add(item);
		}
		return Success(ret);
	}
}
