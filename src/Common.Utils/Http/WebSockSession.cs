using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace Common.Utils
{

	internal class WebSockSession : IDisposable
	{
		private readonly ILogger logger;

		public string sessionID { get; }
		public WebSocket sock { get; set; }
		public string remoteIP { get; set; }

		public DateTime lastActivate { get; set; }

		public WebSockManager owner { get; }

		public WebSockSession(WebSocket sock, string remoteIP, WebSockManager owner, ILogger logger)
		{
			sessionID = MyRandom.RandomString(30);
			this.sock = sock;
			this.owner = owner;
			this.logger = logger;
			lastActivate = DateTime.Now;

			_ = this.SendAsync($"ping {DateTime.UtcNow:HH:mm:ss}");

			Thread th = new Thread(new ParameterizedThreadStart(async (x) => await Worker(x)))
			{
				IsBackground = true,
				Name = "WebSocketSession.Worker"
			};
			th.Start(this);
			this.remoteIP = remoteIP;
		}

		public async Task SendAsync(string msg)
		{
			await SendAsync(msg, CancellationToken.None);
		}

		private async Task SendAsync(string msg, CancellationToken ct)
		{
			if (sock != null && sock.State == WebSocketState.Open)
			{
				await sock.SendAsync(msg.GetBytesUtf8(), WebSocketMessageType.Text, true, ct);
			}
		}

		private static async Task Worker(object? x)
		{
			if (x is not WebSockSession sess)
			{
				return;
			}
			while (sess.sock?.State == WebSocketState.Open)
			{
				try
				{
					using MemoryStream ms = new MemoryStream();
					WebSocketReceiveResult recv;
					do
					{
						byte[] buf = new byte[4 * 1024];
						recv = await sess.sock.ReceiveAsync(buf, CancellationToken.None);
						ms.Write(buf, 0, recv.Count);
					}
					while (!recv.EndOfMessage);

					if (ms.Length > 0)
					{
						var msg = ms.ToArray().GetStringUtf8();
						try
						{
							sess.owner.OnReceive(sess.sessionID, msg);
						}
						catch
						{
							//ignore and continue.
						}
					}

				}
				catch
				{
					//contine until websocket.state != open
					continue;
				}
			}
		}

		public void Dispose()
		{
			sock.Dispose();

			GC.SuppressFinalize(this);
		}

		internal async Task DisconnectAsync(WebSocketCloseStatus status, string statusDescr, CancellationToken ct)
		{
			await sock.CloseAsync(status, statusDescr, ct);
		}
	}

}
