using BazarServer.Application.PeerServers;
using BazarServer.Entity.PeerServers;
using BazarServer.Entity.SeedWork;
using BazarServer.Entity.Storage;
using MediatR;

namespace BazarServer.Controllers;

/// <summary>
/// sync data with peer servers.
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public partial class PeerController : BazarControllerBase
{
	private readonly ILogger<UserCommandController> _logger;

	ICommandRepository commandRepository;
	IUserRepository userRepository;
	IPostRepository postRepository;
	IPeerManager peerManager;
	IAntiSpam antiSpam;

	public PeerController(ILogger<UserCommandController> logger, ICommandRepository commandRepository, IUserRepository userRepository, IPostRepository postRepository, IPeerManager peerManager, IAntiSpam antiSpam)
	{
		_logger = logger;
		this.commandRepository = commandRepository;
		this.userRepository = userRepository;
		this.postRepository = postRepository;
		this.peerManager = peerManager;
		this.antiSpam = antiSpam;
	}

	/// <summary>
	/// register a BazarServer to this instance
	/// </summary>
	/// <param name="baseUrl"></param>
	/// <returns></returns>
	[HttpPost]
	public async Task<ApiResponse> RegisterPeer([FromForm] string baseUrl)
	{
		var ret = await peerManager.RegisterAsync(baseUrl);
		if (!ret.success)
		{
			return Error(ret.msg);
		}

		return Success();
	}

	/// <summary>
	/// query user command based on receiveOffset. result will not include lastOffset.
	/// this function will have internal cache and therefore possible delay.
	/// </summary>
	/// <param name="lastOffset"></param>
	/// <param name="forwardCount">max 1000</param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<UserCommand>>> RetrieveCommandBatch(long lastOffset, int forwardCount)
	{
		int limit = 1000;
		if (forwardCount > limit)
		{
			return Error<List<UserCommand>>($"forwardCount exceed limit {limit}");
		}
		IEnumerable<UserCommand>? ay = await RetrieveCommandWithCache(lastOffset, forwardCount);
		if (ay == null)
		{
			return Error<List<UserCommand>>("fail to query return null");
		}
		return Success(ay.ToList());
	}

	private async Task<IEnumerable<UserCommand>?> RetrieveCommandWithCache(long lastOffset, int forwardCount)
	{
		string key = $"GetCommandWithCache_{lastOffset}_{forwardCount}";
		var ret = await CacheHelper.WithCacheAsync(
									key,
									async () => await peerManager.RetrieveUserCommandBatch(lastOffset, forwardCount),
									5000,
									false
									);
		return ret;
	}

	/// <summary>
	/// get all known peers
	/// </summary>
	/// <param name="topN">top N servers of highest Reputation</param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ApiResponse<List<PeerServerDto>>> GetPeerList(int topN = 100)
	{
		List<PeerServerDto> ret = new();
		var list = peerManager.GetAllServers(topN);
		foreach (var item in list)
		{
			PeerServerDto dto = new PeerServerDto(item.BaseUrl)
			{
				Reputation = item.Reputation,
				ReceiveCount = item.ReceiveCount,
				ReceiveDupCount = item.ReceiveDupCount,
				ReceiveErrorCount = item.ReceiveErrorCount,
				ReceiveOkCount = item.ReceiveOkCount,
			};
			ret.Add(dto);
		}
		await Task.CompletedTask;
		return Success(ret);
	}
}
