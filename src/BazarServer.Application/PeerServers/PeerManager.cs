using BazarServer.Application.Commands;
using BazarServer.Application.Common;
using BazarServer.Entity.Commands;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BazarServer.Application.PeerServers
{
	public class PeerManager : IPeerManager
	{
		IPeerServerRepository peerServerRepository;
		ILogger<PeerManager> logger;
		IUserRepository userRepository;
		ICommandRepository commandRepository;
		ICommandManager commandManager;

		string selfBaseUrl;

		/// <summary>
		/// server baseUrl as key
		/// </summary>
		ConcurrentDictionary<string, PeerServerStat> dicServerStats = new();

		public PeerManager(IPeerServerRepository peerServerRepository, ILogger<PeerManager> logger, IUserRepository userRepository, ICommandRepository commandRepository, IConfiguration configuration, ICommandManager commandManager)
		{
			this.peerServerRepository = peerServerRepository;
			this.logger = logger;
			this.userRepository = userRepository;
			this.commandRepository = commandRepository;

			selfBaseUrl = ConfigHelper.GetConfigValue(configuration, "BazarBaseUrl");
			selfBaseUrl = RegulateUrl(selfBaseUrl);
			if (!selfBaseUrl.StartsWith("https://"))
			{
				throw new Exception("selfBaseUrl must start with https");
			}

			LoadPeers();

			new Thread(new ParameterizedThreadStart(async (x) => await ThreadTimerProc(x)))
			{
				IsBackground = true,
				Name = "PeerTimer"
			}
			.Start(this);
			this.commandManager = commandManager;
		}

		private static string RegulateUrl(string url)
		{
			url = url.ToLower();
			if (!url.EndsWith('/'))
			{
				url += '/';
			}
			return url;
		}

		private async Task ThreadTimerProc(object? obj)
		{
			while (true)
			{
				try
				{
					foreach (var stat in dicServerStats.Values)
					{
						var url = stat.server.BaseUrl;
						await TimerOne(stat);
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "");
				}
				await Task.Delay(100);
			}
		}

		ConcurrentDictionary<string, long> counters = new ConcurrentDictionary<string, long>();

		private async Task TimerOne(PeerServerStat stat)
		{
			try
			{
				if (DateTime.Now < stat.server.nextRetrieveTime)
				{
					return;
				}

				var ret = await RefreshCommandAsync(stat);
				if (ret.success)
				{
					var ss = stat.server.BaseUrl;
					stat.server.IncreaseReputation(100000);
				}
				else
				{
					stat.server.ReduceReputation(10000);
				}

				//If no more data, postpone sometime.
				//As of "Six Degrees of Separation" theory, one message may takes at most 5*6=30 seconds to spread every corner.
				if (ret.total == 0)
				{
					stat.server.nextRetrieveTime = DateTime.Now + TimeSpan.FromSeconds(5);
				}
				else
				{
					//for every OK-msg will postpone 100 milli, refers to 10 effective msg per seconds.
					stat.server.nextRetrieveTime = DateTime.Now + TimeSpan.FromSeconds(ret.okCount * 0.1);
				}

				if (ret.okCount > 0)
				{
					//faster fetch for whitelist. currently every peer is in whitelist.
					stat.server.nextRetrieveTime = DateTime.Now;
				}

				var count = counters.GetOrAdd(stat.server.BaseUrl, 0);
				if (count % 12 == 0)
				{
					await peerServerRepository.SaveAsync(stat.server);

					await RefreshServerListAsync(stat);
				}
				counters[stat.server.BaseUrl] += 1;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"url: {stat.server.BaseUrl}");
			}
			return;
		}

		private async Task RefreshServerListAsync(PeerServerStat stat)
		{
			var ret = await GetPeerListAsync(stat.server);
			if (ret.success && ret.data != null)
			{
				foreach (var item in ret.data)
				{
					item.baseUrl = RegulateUrl(item.baseUrl);

					if (item.baseUrl == selfBaseUrl)
					{
						continue;
					}
					if (!item.baseUrl.StartsWith("https://"))
					{
						continue;
					}
					if (item.Reputation < 0.01)
					{
						continue;
					}

					if (!dicServerStats.ContainsKey(item.baseUrl))
					{
						var pss = await AddNewServer(item.baseUrl);
					}
				}

				if (!ret.data.Exists(x => x.baseUrl == selfBaseUrl))
				{
					await RegisterToRemoteAsync(stat.server, selfBaseUrl);
				}
			}
		}

		/// <summary>
		/// retrieve commands from remote and update local
		/// </summary>
		/// <param name="pss"></param>
		/// <returns></returns>
		public async Task<(bool success, int total, int okCount)> RefreshCommandAsync(PeerServerStat pss)
		{
			int total = 0;
			int okCount = 0;
			try
			{
				var url = Url.Combine(pss.server.BaseUrl, "/Peer/RetrieveCommandBatch");
				url += $"?lastOffset={pss.server.lastReceiveOffset}&forwardCount=100";

				logger.LogInformation($"{pss.server.BaseUrl}, offset={pss.server.lastReceiveOffset}");

				var ss = HttpHelper.Get(url);

				var res = Json.Deserialize<ApiResponse<List<UserCommand>>>(ss);
				if (res == null || !res.success || res.data == null)
				{
					return (false, total, okCount);
				}
				var ay1 = res.data.Select(x => x.commandID);
				var ay2 = res.data.Select(x => x.receiveOffset);
				List<MsgResult> ayres = new List<MsgResult>();
				total = res.data.Count;
				foreach (var cmd in res.data)
				{
					try
					{
						var ret = await Peer_OnMessage(pss, cmd);
						if (ret == MsgResult.OK)
						{
							okCount++;
						}
						else if (ret != MsgResult.Dup)
						{
							var ccc = ret;
						}
						ayres.Add(ret);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "");
					}

					if (cmd.receiveOffset > pss.server.lastReceiveOffset)
					{
						pss.server.lastReceiveOffset = cmd.receiveOffset;
					}
				}
				logger.LogInformation($"succeed RefreshCommandAsync {pss.server.BaseUrl}, total={total}, ok={okCount}, lastOffset={pss.server.lastReceiveOffset}");
				return (true, total, okCount);
			}
			catch (HttpRequestException)
			{
				// normal situation
				//logger.LogInformation($"fail RefreshCommandAsync {pss.server.BaseUrl}");
				return (false, total, okCount);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "");
				return (false, total, okCount);
			}
		}

		/// <summary>
		/// register baseUrl to a remote server
		/// </summary>
		/// <param name="server"></param>
		/// <param name="selfBaseUrl"></param>
		/// <returns></returns>
		private async Task RegisterToRemoteAsync(PeerServer server, string selfBaseUrl)
		{
			try
			{
				var url = Url.Combine(server.BaseUrl, $"/Peer/RegisterPeer");
				var dic = new Dictionary<string, string>
				{
					{ "baseUrl", selfBaseUrl }
				};
				await HttpHelper.PostFormAsync(url, dic);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "");
			}
		}

		/// <summary>
		/// get peerList from one remote server
		/// </summary>
		/// <param name="server"></param>
		/// <returns></returns>
		private async Task<ApiResponse<List<PeerServerDto>>> GetPeerListAsync(PeerServer server)
		{
			try
			{
				var url = Url.Combine(server.BaseUrl, $"/Peer/GetPeerList");
				var ss = await HttpHelper.GetAsync(url);
				var ret = Json.Deserialize<ApiResponse<List<PeerServerDto>>>(ss);
				if (ret == null)
				{
					return new ApiResponse<List<PeerServerDto>>(false, "fail to Deserialize");
				}
				return ret;
			}
			catch (HttpRequestException ex)
			{
				// normal situation
				logger.LogInformation($"fail GetPeerListAsync {server.BaseUrl}");
				return new ApiResponse<List<PeerServerDto>>(false, ex.Message);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "");
				return new ApiResponse<List<PeerServerDto>>(false, ex.Message);
			}
		}

		private void LoadPeers()
		{
			var all = peerServerRepository.GetAll();

			foreach (var item in all)
			{
				if (!string.IsNullOrEmpty(item.BaseUrl))
				{
					if (item.Reputation > 1)
					{
						item.Reputation = 1;
					}
					dicServerStats[item.BaseUrl] = new PeerServerStat(this, item, logger);
				}
			}
		}

		/// <summary>
		/// 0.6 means 60% true
		/// </summary>
		/// <param name="publishRateNew"></param>
		/// <returns></returns>
		protected static bool RandomRate(double publishRateNew)
		{
			int resolution = 10000;
			int val = (int)(publishRateNew * resolution);
			var rand = MyRandom.Random(0, resolution);
			return (val > rand);
		}

		public async Task<List<UserCommand>> RetrieveUserCommandBatch(long lastOffset, int forwardCount)
		{
			List<UserCommand> ay = await commandRepository.Batch(lastOffset, forwardCount);
			return ay;
		}

		public int GetServerCount()
		{
			return dicServerStats.Count;
		}

		public async Task<(bool success, string msg)> RegisterAsync(string baseUrl)
		{
			baseUrl = RegulateUrl(baseUrl);

			if (!baseUrl.StartsWith("https://"))
			{
				return (false, "baseUrl must start with https");
			}

			if (dicServerStats.ContainsKey(baseUrl))
			{
				return (false, "dup");
			}

			try
			{
				var url = Url.Combine(baseUrl, "Health");
				var ss = await HttpHelper.GetAsync(url);
			}
			catch (Exception ex)
			{
				return (false, $"check health fail: {ex.Message}");
			}

			var pss = await AddNewServer(baseUrl);

			return (true, "");
		}

		private async Task<PeerServerStat> AddNewServer(string baseUrl)
		{
			var server = new PeerServer() { BaseUrl = baseUrl };
			await peerServerRepository.SaveAsync(server);
			var pss = new PeerServerStat(this, server, logger);
			dicServerStats.TryAdd(baseUrl, pss);
			return pss;
		}

		static SemaphoreSlim sem = new SemaphoreSlim(1, 1);

		public enum MsgResult
		{
			Error = 0,
			Dup = 1,
			OK = 2
		}

		/// <summary>
		/// this function may be called in multi-thread
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public async Task<MsgResult> Peer_OnMessage(PeerServerStat stat, UserCommand cmd)
		{
			await sem.WaitAsync();
			try
			{
				stat.server.ReceiveCount++;

				cmd.receiveTime = DateHelper.CurrentTimeMillis();

				var oldCmd = await commandRepository.GetCommandAsync_WithCache(cmd.commandID);
				if (oldCmd != null)
				{
					stat.server.ReceiveDupCount++;
					return MsgResult.Dup;
				}

				var user = await userRepository.GetUserInfoAsync_WithCache(cmd.userID);
				var publicKey = "";
				if (user != null)
				{
					publicKey = user.publicKey;
				}
				else
				{
					if (cmd.commandType == "UserInfo")
					{
						var userCmd = Json.Deserialize<UserInfoCmd>(cmd.commandContent);
						if (userCmd != null)
						{
							publicKey = userCmd.publicKey;
						}
					}
				}
				if (string.IsNullOrEmpty(publicKey))
				{
					stat.server.ReceiveErrorCount++;
					return MsgResult.Error;
				}

				if (!ValidationHelper.Validate(cmd, out var _))
				{
					stat.server.ReceiveErrorCount++;
					return MsgResult.Error;
				}

				if (!Encryption.CheckSignature(cmd.commandContent, cmd.signature, publicKey))
				{
					stat.server.ReceiveErrorCount++;
					return MsgResult.Error;
				}

				stat.server.ReceiveOkCount++;

				await commandManager.SaveAndDispatch(cmd, stat.server.BaseUrl);

				return MsgResult.OK;

			}
			finally
			{
				sem.Release();
			}

		}


		public List<PeerServer> GetAllServers(int topNReputation)
		{
			return dicServerStats.Values.Select(x => x.server).OrderByDescending(x => x.Reputation).Take(topNReputation).ToList();
		}
	}
}
