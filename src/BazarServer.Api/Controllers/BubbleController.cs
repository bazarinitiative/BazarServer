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

	char[] nochar = { '.', ',', '!', '?' };
	List<string> nosearch = new List<string>()
	{
		"Deleted", "the", "is", "a", "in", "on", "to", "been", "be", "at", "what", "you", "me", "of", "the", "have",
		"who", "from", "should", "not", "will", "can", "that", "and", "there's", "that's", "it's", "this", "my", "your",
		"his", "their", "so", "for", "if", "has", "had", "when", "where", "they", "them", "over", "which", "how", "our",
		"out", "here", "there", "very", "she", "with", "own", "why", "himself", "all", "some", "any", "would", "more",
		"but", "into", "most", "off", "are", "than", "then", "him", "her", "did", "about", "was"
	};

	bool canTrend(string ss)
	{
		var keepSearch = true;
		if (ss.Length < 3)
		{
			keepSearch = false;
		}
		if (ss.GetBytesUtf8().Length > 20)
		{
			keepSearch = false;
		}
		if (ss.Any(x => "~!@#$%^&*():<>?,./;'[]{}|-=\\\"，。？".Contains(x)))
		{
			keepSearch = false;
		}
		return keepSearch;
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
	public async Task<ApiResponse<List<TrendUnit>>> Trending(string? userID = "", long queryTime = 0, string? token = "", int count = 5)
	{
		if (count > 100)
		{
			count = 100;
		}
		var users = (await userRepository.GetRandomUser(20)).OrderByDescending(x=>x.commandTime).Take(10);
		var posts = (await postRepository.GetRandomPost(100)).OrderByDescending(x=>x.commandTime).Take(50);
		HashSet<string> set = new HashSet<string>();
		foreach (var user in users)
		{
			var sub = user.userName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			foreach (var item in sub)
			{
				var ss = item.Trim().Trim(nochar);
				if (canTrend(ss))
				{
					set.Add(ss);
				}
			}
		}
		foreach (var post in posts)
		{
			var sub = post.content.Replace('\n', ' ').Replace('\'', ' ').Replace('’', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
			foreach (var item in sub)
			{
				var ss = item.Trim().Trim(nochar);
				if (canTrend(ss))
				{
					set.Add(ss);
				}
			}
		}
		foreach (var item in nosearch)
		{
			set.Remove(item.FirstCharUpper());
			set.Remove(item.FirstCharLower());
		}
		set.Remove("");

		List<string> pool = set.ToList();
		List<TrendUnit> ret = new List<TrendUnit>();
		HashSet<string> added = new HashSet<string>();
		for (int i = 0; i < count; i++)
		{
			int idx = MyRandom.Random(0, pool.Count);
			if (added.Contains(pool[idx]))
			{
				i--;
				continue;
			}
			added.Add(pool[idx]);
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
