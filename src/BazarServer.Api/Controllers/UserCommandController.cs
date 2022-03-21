using BazarServer.Application.Commands;
using BazarServer.Entity.Commands;
using BazarServer.Entity.SeedWork;
using BazarServer.Entity.Storage;
using MediatR;

namespace BazarServer.Controllers;

/// <summary>
/// Execute command from user. Such as post, follow, like, channel, etc.
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class UserCommandController : BazarControllerBase
{
	private readonly ILogger<UserCommandController> _logger;
	ICommandRepository commandRepository;
	IUserRepository userRepository;
	ICommandManager commandManager;
	IAntiSpam antiSpam;

	public UserCommandController(ILogger<UserCommandController> logger, ICommandRepository commandRepository, ICommandManager commandManager, IAntiSpam antiSpam, IUserRepository userRepository)
	{
		_logger = logger;
		this.commandRepository = commandRepository;
		this.commandManager = commandManager;
		this.antiSpam = antiSpam;
		this.userRepository = userRepository;
	}

	public class ModelShowDto
	{
		public UserInfoCmd? userInfo { get; set; }
		public UserPicCmd? userPic { get; set; }
		public ChannelCmd? channel { get; set; }
		public ChannelMemberCmd? channelMember { get; set; }
		public FollowingCmd? following { get; set; }

		public PostCmd? post { get; set; }
		public LikeCmd? like { get; set; }

		public DeleteCmd? delete { get; set; }
	}

	/// <summary>
	/// this is an empty function, just to show how commandContent model should be.
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	public ApiResponse<ModelShowDto> ModelShow()
	{
		return Success(new ModelShowDto());
	}

	/// <summary>
	/// execute a user command
	/// </summary>
	/// <param name="req"></param>
	/// <returns></returns>
	[HttpPost]
	public async Task<ApiResponse<UserCommandRespDto>> CommandAsync(UserCommandRequestModel req)
	{
		var dd = await Preprocess(req);
		if (!dd.success)
		{
			return new ApiResponse<UserCommandRespDto>(false, dd.msg);
		}

		string ip = HttpContext.GetRealIP();

		var cmd = req.ToUserCommand();
		var res = await commandManager.SaveAndDispatch(cmd, ip);
		if (!res.success)
		{
			return Error(res.msg, res.resp);
		}

		return Success(res.resp);
	}

	/// <summary>
	/// command level verification
	/// </summary>
	/// <param name="req"></param>
	/// <returns></returns>
	private async Task<(bool success, string msg)> Preprocess(UserCommandRequestModel req)
	{
		var spam = antiSpam.Check("Client.Command", HttpContext.GetRealIP());
		if (!spam.success)
		{
			return (false, spam.msg);
		}

		if (!ValidationHelper.Validate(req, out var results))
		{
			return (false, $"validate fail: {results.FirstOrDefault()}");
		}

		var user = await userRepository.GetUserInfoAsync(req.userID);
		if (user == null)
		{
			if (req.commandType == "UserInfo")
			{
				user = Json.Deserialize<UserInfo>(req.commandContent);
				if (user == null)
				{
					return (false, "invalid userInfo");
				}
				user.publicKey = user.publicKey.Trim();
			}
		}
		if (user == null)
		{
			return (false, $"User not exist: {req.userID}");
		}

		bool check = Encryption.CheckSignature(req.commandContent, req.signature, user.publicKey);
		if (!check)
		{
			return (false, "signature check fail");
		}

		if (!req.IsValidContentLength())
		{
			return (false, $"req.commandContent too long: {req.commandContent.Length}");
		}

		var cmd = req.ToUserCommand();
		var oldCmd = await commandRepository.GetCommandAsync_WithCache(cmd.commandID);
		if (oldCmd != null)
		{
			return (false, $"duplicate commandID: {req.commandID}");
		}

		return (true, "ok");
	}
}
