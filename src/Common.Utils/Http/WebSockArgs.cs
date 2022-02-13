namespace Common.Utils
{
	public class WebSockArgs : EventArgs
	{
		public string sessionID { get; set; } = "";

		/// <summary>
		/// remoteIP at OnConnected. OnDisconnect
		/// msg body at OnMessage.
		/// </summary>
		public string msg { get; set; } = "";
	}
}
