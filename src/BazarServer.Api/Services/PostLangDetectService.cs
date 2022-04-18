using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using BazarServer.Infrastructure.Storage;

namespace BazarServer.Services
{
	public class PostLangDetectService : IHostedService
	{
		bool running = true;
		Thread? thread = null;
		IPostRepository postRepository;
		ILogger<PostLangDetectService> logger;
		IGenericMongoCollection<PostLangDetect> _connDetect;
		const int detectLen = 20;
		string apiKey = "";

		public PostLangDetectService(IPostRepository postRepository, ILogger<PostLangDetectService> logger, IGenericMongoCollection<PostLangDetect> connDetect)
		{
			this.postRepository = postRepository;
			this.logger = logger;
			_connDetect = connDetect;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			apiKey = ConfigHelper.GetConfigValue(null, "BazarTranslateKey");
			thread = new Thread(async () => { await Worder(); });
			thread.Start();
			return Task.CompletedTask;
		}

		long lastPostTime = 0;

		private async Task Worder()
		{
			while (running)
			{
				try
				{
					var ay = await postRepository.GetPostNoLang(lastPostTime, 10);
					foreach (var post in ay)
					{
						var ret = await DetectLang(post.content);
						if (!ret.success)
						{
							logger.LogWarning("fail to DetectLangGG: " + ret.msg);
							await Task.Delay(1000);
							break;
						}
						PostLangDetect detect = new PostLangDetect()
						{
							postID = post.postID,
							detectLength = detectLen,
							detectResult = ret.lang,
							detectTime = DateTime.Now,
						};
						await _connDetect.UpsertAsync(x=>x.postID == detect.postID, detect);
						post.contentLang = ret.lang;
						await postRepository.SaveAsync(post);
						if (post.commandTime > lastPostTime)
						{
							lastPostTime = post.commandTime;
						}
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex.ToString());
					await Task.Delay(1000);
				}
				await Task.Delay(50);
			}
		}

		#region google modals

		class TranslateReq
		{
			public string q { get; set; } = "";
		}

		class TranslateResp
		{
			public TranslateRespData data { get; set; } = new TranslateRespData();
		}

		class TranslateRespData
		{
			public List<List<TranslateRespUnit>> detections { get; set; } = new List<List<TranslateRespUnit>>();
		}

		class TranslateRespUnit
		{
			/// <summary>
			/// 
			/// </summary>
			public object? confidence { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public object? isReliable { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public string language { get; set; } = "";
		}

		#endregion

		/// <summary>
		/// detect first N char for language from google or somewhere
		/// </summary>
		/// <param name="str"></param>
		/// <returns>en, fr, zh, ja, etc</returns>
		private async Task<(bool success, string msg, string lang)> DetectLang(string str)
		{
			try
			{
				if (str.Length == 0)
				{
					return (true, "", "**");
				}

				TranslateReq tm = new TranslateReq();
				tm.q = str.Left(detectLen);
				var json = Json.Serialize(tm);

				var url = "https://translation.googleapis.com/language/translate/v2/detect?key=" + apiKey;
				var ret = await HttpHelper.PostAsync(url, json);

				TranslateResp? resp = Json.Deserialize<TranslateResp>(ret);
				if (resp == null || resp.data.detections.Count == 0)
				{
					return (false, "detect fail: " + ret, "");
				}
				var lang = resp.data.detections[0][0].language.Split('-')[0];
				return (true, "", lang);
			}
			catch (Exception ex)
			{
				return (false, ex.Message, "");
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			running = false;
			if (thread != null)
			{
				thread.Join();
				thread = null;
			}
			return Task.CompletedTask;
		}
	}
}
