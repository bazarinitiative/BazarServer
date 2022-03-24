using BazarServer.Application.Posts;
using BazarServer.Application.Users;
using BazarServer.Entity.SeedWork;
using BazarServer.Entity.Storage;
using MediatR;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace BazarServer.Controllers;

/// <summary>
/// provide different kind of queries
/// </summary>
[UserQueryFilter]
[ApiController]
[Route("[controller]/[action]")]
public partial class UserQueryController : BazarControllerBase
{
	private readonly ILogger<UserCommandController> _logger;
	IUserRepository userRepository;
	IPostRepository postRepository;
	ICommandRepository commandRepository;

	public UserQueryController(ILogger<UserCommandController> logger, IUserRepository userRepository, IPostRepository postRepository, ICommandRepository commandRepository)
	{
		_logger = logger;
		this.userRepository = userRepository;
		this.postRepository = postRepository;
		this.commandRepository = commandRepository;
	}

	/// <summary>
	/// get basic info of one user.
	/// </summary>
	/// <param name="userID"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<UserInfo>> GetUserInfoAsync(string userID)
	{
		if (string.IsNullOrEmpty(userID))
		{
			return Error<UserInfo>("userID param empty");
		}

		var user = await userRepository.GetUserInfoAsync(userID);
		if (user == null)
		{
			return Error<UserInfo>("user not found");
		}

		if (user.createTime == 0)
		{
			user.createTime = user.commandTime;
		}

		return Success(user);
	}

	/// <summary>
	/// get avatar pic of one user
	/// </summary>
	/// <param name="userID"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<UserPic>> GetUserPicAsync(string userID)
	{
		if (string.IsNullOrEmpty(userID))
		{
			return Error<UserPic>("userID param empty");
		}

		var user = await userRepository.GetUserPicAsync(userID);
		if (user == null)
		{
			return Error<UserPic>("user not found");
		}

		return Success(user);
	}

	/// <summary>
	/// timeline data for public.
	/// 
	/// this is just a demo result. real data should combine with recommendation and moderation.
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="page"></param>
	/// <param name="pageSize"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<PostDto>>> PublicTimeline(string? userID, int page = 0, int pageSize = 20)
	{
		if (userID == null)
		{
			userID = "";
		}

		List<Post> ay = await postRepository.TimelineAsync(page, pageSize);
		var ret = await PostQueryFacade.GetPostDto(postRepository, userID, ay);

		return Success(ret);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="queryTime"></param>
	/// <param name="token"></param>
	/// <param name="page"></param>
	/// <param name="pageSize"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<PostDto>>> HomeLine(string userID, long queryTime, string token, int page, int pageSize)
	{
		var check = await UserQueryFacade.CheckQuery(userRepository, userID, queryTime, token);
		if (!check.success)
		{
			return Error<List<PostDto>>(check.msg);
		}

		List<PostDto> ret = await GetHomeLine(userID, page, pageSize);
		return Success(ret);
	}

	private async Task<List<PostDto>> GetHomeLine(string userID, int page, int pageSize)
	{
		var users = await userRepository.GetUserFollowees(userID, 0, 1000);
		var ay = users.Select(x => x.targetID).ToList();
		ay.Add(userID);
		var posts = await PostQueryFacade.GetLatestPostsByUsers(postRepository, ay, page, pageSize);
		var ret = await PostQueryFacade.GetPostDto(postRepository, userID, posts);
		return ret;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="targetID"></param>
	/// <returns>will return empty data if not following</returns>
	[HttpGet]
	public async Task<ApiResponse<Following>> GetFollowing(string userID, string targetID)
	{
		var following = await userRepository.GetUserFollowing(userID, targetID);
		if (following == null)
		{
			following = new Following();
		}
		return Success<Following>(following);
	}

	/// <summary>
	/// get userID follow whom
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="page"></param>
	/// <param name="pageSize"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<UserDto>>> GetFollowees(string userID, int page, int pageSize)
	{
		List<UserDto> ret = new List<UserDto>();
		var ay = await userRepository.GetUserFollowees(userID, page, pageSize);
		foreach (var item in ay)
		{
			var dto = await UserQueryFacade.GetUserDto_WithCache(userRepository, item.targetID);
			ret.Add(dto);
		}
		return Success(ret);
	}

	/// <summary>
	/// get who follow userID
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="page"></param>
	/// <param name="pageSize"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<UserDto>>> GetFollowers(string userID, int page, int pageSize)
	{
		List<UserDto> ret = new List<UserDto>();
		var ay = await userRepository.GetUserFollowers(userID, page, pageSize);
		foreach (var item in ay)
		{
			var dto = await UserQueryFacade.GetUserDto_WithCache(userRepository, item.userID);
			ret.Add(dto);
		}
		return Success(ret);
	}

	/// <summary>
	/// get posts of one user. latest at top.
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="onlyOriginalPost">without reply and repost</param>
	/// <param name="page">start from 0</param>
	/// <param name="pageSize"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<PostDto>>> GetPostsAsync(string userID, bool onlyOriginalPost = true, int page = 0, int pageSize = 5)
	{
		List<Post> ay = await postRepository.GetPostsByUserAsync(userID, onlyOriginalPost, page, pageSize);
		var ret = await PostQueryFacade.GetPostDto(postRepository, userID, ay);

		return Success(ret);
	}

	/// <summary>
	/// get data of this exact post.
	/// </summary>
	/// <param name="postID"></param>
	/// <param name="userID">input to get if this user had liked the post</param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<PostDto>> GetPostSimple(string postID, string? userID)
	{
		var post = await postRepository.GetPostAsync(postID);
		if (post == null)
		{
			return Error<PostDto>("post not exist");
		}
		var ps = await postRepository.GetPostStatisticAsync(postID);
		if (ps == null)
		{
			ps = new PostStatistic(postID);
		}
		var liked = false;
		if (!string.IsNullOrEmpty(userID))
		{
			liked = await postRepository.GetPostLikeAsync(userID, postID);
		}
		var replyToUser = "";
		if (post.replyTo.Length > 0)
		{
			var rpost = await postRepository.GetPostAsync(post.replyTo);
			replyToUser = rpost?.userID ?? "";
		}
		PostDto dto = new PostDto(post, ps, liked, replyToUser);
		return Success(dto);
	}

	/// <summary>
	/// related detail data of one post. include current, parentPost, threadPost, replies.
	/// parentPost and threadPost may be null, if not exist or same with current.
	/// 
	/// this is just a simple demo page. real data should combine with recommendation and moderation.
	/// </summary>
	/// <param name="postID"></param>
	/// <param name="page"></param>
	/// <param name="pageSize"></param>
	/// <param name="userID">input to get if this user had liked the related posts</param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<PostDetailDto>> GetPostDetail(string postID, int page = 0, int pageSize = 5, string userID = "")
	{
		var post = await postRepository.GetPostAsync(postID);
		if (post == null)
		{
			return Error<PostDetailDto>("post not exist");
		}
		List<string> replies = await postRepository.GetRepliesAsync(postID, page, pageSize);

		List<string> postIDs = replies.ToList();
		postIDs.Add(postID);
		if (post.replyTo.Length > 0)
		{
			postIDs.Add(post.replyTo);
		}
		if (post.threadID != post.postID && post.threadID != post.replyTo)
		{
			postIDs.Add(post.threadID);
		}
		var dicPosts = await postRepository.GetPostsAsync(postIDs);
		var ayDto = await PostQueryFacade.GetPostDto(postRepository, userID, dicPosts.Values.ToList());
		var dicDto = ayDto.ToDictionary(x => x.post.postID);

		var current = dicDto[post.postID];
		PostDetailDto ret = new PostDetailDto() { current = current };
		if (post.replyTo.Length > 0)
		{
			var id = post.replyTo;
			ret.parent = dicDto[id];
		}
		if (post.threadID != post.postID && post.threadID != post.replyTo)
		{
			var id = post.threadID;
			ret.thread = dicDto[id];
		}
		foreach (var id in replies)
		{
			ret.replies.Add(dicDto[id]);
		}
		var ss = ret.ToJsonString();
		return Success(ret);
	}

	/// <summary>
	/// get profile of one user
	/// </summary>
	/// <param name="userID"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userID)
	{
		var userInfo = await userRepository.GetUserInfoAsync(userID);
		if (userInfo == null)
		{
			return Error<UserProfileDto>("user not found");
		}
		var userStatistic = await userRepository.GetUserStatisticAsync(userID);

		var obj = new UserProfileDto(userInfo, userStatistic ?? new UserStatistic(userID));
		return Success(obj);
	}

	/// <summary>
	/// get a command. client need this to upload data to another server by UserInfo or Post function.
	/// peer server can use this retrieve some data.
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<UserCommand>> GetCommand(string commandID)
	{
		var ret = await commandRepository.GetCommandAsync(commandID);
		if (ret == null)
		{
			return Error("not found", ret);
		}
		else
		{
			return Success(ret);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="userID"></param>
	/// <param name="queryTime">time of query. for check. in milli</param>
	/// <param name="token">should be signature of queryTime.ToString()</param>
	/// <param name="startTime">startTime (include) of backwards. 0 means now.</param>
	/// <param name="maxCount"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<NotifyMessage>>> GetNotifies(string userID, long queryTime, string token, long startTime = 0, int maxCount = 20)
	{
		if (startTime == 0)
		{
			startTime = DateHelper.CurrentTimeMillis();
		}
		var check = await UserQueryFacade.CheckQuery(userRepository, userID, queryTime, token);
		if (!check.success)
		{
			return Error<List<NotifyMessage>>($"{check.msg}");
		}

		//it is possible that we return less notifyMsg in some rare situation, it's ok. may do better later.
		var ret = await userRepository.GetUserNotify(userID, startTime + 1, maxCount);
		var ret2 = ret.Where(x => x.fromWho != "").ToList();

		if (ret.Count > 0)
		{
			long maxTime = ret.Max(x => x.notifyTime);
			await userRepository.TryUpdateNotifyGetTime(userID, maxTime);
		}
		return Success(ret2);
	}

	/// <summary>
	/// get new notify count, since last query notify
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<long>> GetNewNotifyCount(string userID, long queryTime, string token)
	{
		var check = await UserQueryFacade.CheckQuery(userRepository, userID, queryTime, token);
		if (!check.success)
		{
			return Error<long>($"{check.msg}");
		}

		var ret = await userRepository.GetNewNotifyCount(userID);
		return Success(ret);
	}

	/// <summary>
	/// URL like: https://api.bazar.social/UserQuery/UserPicImage/KrNa6OG2O0KjbVXLzRKuxlFknVE1oH.jpeg
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	[Route("/UserQuery/UserPicImage/{id?}")]
	public async Task<IActionResult> GetUserPicImageAsync()
	{
		string path = Request.Path.Value ?? "";
		var userID = path.Split('/').Last().Replace(".jpeg", "");
		var userPic = await userRepository.GetUserPicAsync(userID);
		if (userPic == null)
		{
			var imgBuf = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "/Static/avatar.png");
			var imgStr = Convert.ToBase64String(imgBuf);
			Response.Headers.ETag = Encryption.Md5Hash(imgStr);
			Response.Headers.LastModified = new DateTime().ToString("r");
			Response.Headers.CacheControl = "max-age=0";
			return new FileContentResult(imgBuf, "image/jpeg");
		}
		var lastModify = DateHelper.FromTimestamp(userPic.commandTime, TimeZoneInfo.Local).ToString("r");
		var sss = Request.Headers.IfModifiedSince;
		if (Request.Headers.TryGetValue("If-Modified-Since", out var since))
		{
			if (DateTime.Parse(since) >= DateTime.Parse(lastModify))
			{
				return StatusCode((int)HttpStatusCode.NotModified);
			}
		}

		var picstr = userPic.pic;
		var buf = Convert.FromBase64String(picstr);
		Response.Headers.ETag = Encryption.Md5Hash(picstr);
		Response.Headers.LastModified = lastModify;
		Response.Headers.CacheControl = "max-age=0";
		return new FileContentResult(buf, "image/jpeg");
	}
}

/// <summary>
/// antiSpam, rateLimit
/// </summary>
internal class UserQueryFilterAttribute : ActionFilterAttribute
{
	public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var svc = context.HttpContext.RequestServices;
		var antiSpam = svc.GetService<IAntiSpam>();

		if (antiSpam == null)
		{
			throw new Exception();
		}

		var spam = antiSpam.Check("Client.Query", context.HttpContext.GetRealIP());
		if (!spam.success)
		{
			context.Result = new JsonResult(new ApiResponse(false, spam.msg));
			return;
		}

		await base.OnActionExecutionAsync(context, next);
	}
}
