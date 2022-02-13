using System.Net.WebSockets;

namespace Common.Utils
{
	public interface IWebSockManager
	{
		event EventHandler<WebSockArgs> OnConnected;
		event EventHandler<WebSockArgs> OnMessage;
		event EventHandler<WebSockArgs> OnDisconnected;

		/// <summary>
		/// if sessionID is still active
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns></returns>
		bool IsActiveSession(string sessionID);

		/// <summary>
		/// 0 will not send ping.
		/// </summary>
		int PingIntervalSeconds { get; set; }

		/// <summary>
		/// how many sessions are there
		/// </summary>
		/// <returns></returns>
		int SessionCount();

		/// <summary>
		/// called by underlying websocket implementation after WebSockets.AcceptWebSocket.
		/// will create a session for this client. return sessionID
		/// </summary>
		/// <param name="client"></param>
		string OnAccept(WebSocket client, string remoteIP);

		/// <summary>
		/// publish msg to all session
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		Task PublishAsync(string msg);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sessionID"></param>
		/// <param name="msg"></param>
		/// <returns></returns>
		Task SendAsync(string sessionID, string msg);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns></returns>
		Task DisconnectAsync(string sessionID, WebSocketCloseStatus status, string statusDescr);
	}
}