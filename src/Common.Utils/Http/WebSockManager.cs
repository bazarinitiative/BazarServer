using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Common.Utils
{
	/// <summary>
	/// for one business, usually one ws url.
	/// </summary>
	public class WebSockManager : IWebSockManager
	{
		/// <summary>
		/// 
		/// </summary>
		ConcurrentDictionary<string, WebSockSession> _dicSession = new ConcurrentDictionary<string, WebSockSession>();

		/// <summary>
		/// 0 will not send ping.
		/// </summary>
		public int PingIntervalSeconds { get; set; } = 10;

		public event EventHandler<WebSockArgs>? OnConnected;
		public event EventHandler<WebSockArgs>? OnMessage;
		public event EventHandler<WebSockArgs>? OnDisconnected;

		ILogger<WebSockManager> _logger;

		public WebSockManager(ILogger<WebSockManager> logger)
		{
			_logger = logger;

			Thread th = new Thread(new ParameterizedThreadStart(async (x) => await Worker(x)));
			th.IsBackground = true;
			th.Start(this);
		}

		/// <summary>
		/// manager need a thread to send heartbeat and remove closed session
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		private async Task Worker(object? obj)
		{
			while (true)
			{
				try
				{
					await WorkOnce();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "");
				}

				await Task.Delay(1000);
			}
		}

		private async Task WorkOnce()
		{
			DateTime now = DateTime.Now;
			var tks = _dicSession.Values.Select(x =>
			{
				return WorkOneSession(x, now);
			});
			await Task.WhenAll(tks);
		}

		private async Task WorkOneSession(WebSockSession sess, DateTime now)
		{
			if (sess.sock == null || sess.sock.State == WebSocketState.Closed || sess.sock.State == WebSocketState.Aborted)
			{
				try
				{
					sess.Dispose();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "");
				}

				_dicSession.TryRemove(sess.sessionID, out var _);
				OnDisconnected?.Invoke(this, new WebSockArgs() { sessionID = sess.sessionID, msg = sess.remoteIP });
			}
			else
			{
				if (sess.sock.State == WebSocketState.Open)
				{
					if (PingIntervalSeconds > 0)
					{
						var diff = (now - sess.lastActivate).TotalSeconds;
						if (diff > PingIntervalSeconds)
						{
							await sess.SendAsync($"ping {DateTime.UtcNow:HH:mm:ss}");
							sess.lastActivate = now;
						}
					}
				}
				else
				{
					try
					{
						await sess.sock.CloseAsync(WebSocketCloseStatus.NormalClosure, $"not open: {sess.sock.State}", CancellationToken.None);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "");
					}
				}
			}
		}

		public string OnAccept(WebSocket client, string remoteIP)
		{
			WebSockSession sess = new WebSockSession(client, remoteIP, this, _logger);

			_dicSession.TryAdd(sess.sessionID, sess);
			OnConnected?.Invoke(this, new WebSockArgs() { sessionID = sess.sessionID, msg = remoteIP });

			return sess.sessionID;
		}

		public async Task DisconnectAsync(string sessionID, WebSocketCloseStatus status, string statusDescr)
		{
			if (_dicSession.TryGetValue(sessionID, out var sess))
			{
				await sess.DisconnectAsync(status, statusDescr, CancellationToken.None);
			}
		}

		public bool IsActiveSession(string sessionID)
		{
			return _dicSession.ContainsKey(sessionID);
		}

		internal void OnReceive(string sessionID, string msg)
		{
			OnMessage?.Invoke(this, new WebSockArgs() { sessionID = sessionID, msg = msg });
		}

		public async Task SendAsync(string sessionID, string msg)
		{
			if (_dicSession.TryGetValue(sessionID, out var session))
			{
				await session.SendAsync(msg);
			}
		}

		public async Task PublishAsync(string msg)
		{
			var tks = _dicSession.Values.Select(x =>
			{
				try
				{
					return x.SendAsync(msg);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "");
					return Task.CompletedTask;
				}
			});

			//should not whenall here, to prevent one slow client affect all.
			if (tks.Any())
			{
				await Task.WhenAny(tks);
			}
		}

		public int SessionCount()
		{
			return _dicSession.Count;
		}
	}
}
