using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// event of users action
	/// </summary>
	public class UserCommand : IStoreData
	{
		/// <summary>
		/// uniqueID of this command
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string commandID { get; set; } = "";

		/// <summary>
		/// seconds since EPOCH. peer declared sending time. may not be true.
		/// </summary>
		public long commandTime { get; set; }

		/// <summary>
		/// who initiative this action
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string userID { get; set; } = "";

		/// <summary>
		/// Post, Following, Repost, Like, Delete, etc...
		/// </summary>
		[StringLength(20)]
		public string commandType { get; set; } = "";

		/// <summary>
		/// origin user request body string
		/// </summary>
		[StringLength(50 * 1024)]
		public string commandContent { get; set; } = "";

		/// <summary>
		/// signature of commandContent.
		/// signed by user with privateKey. can be verified by user publicKey.
		/// </summary>
		[StringLength(200)]
		public string signature { get; set; } = "";

		/// <summary>
		/// when this server receive this command, timemilli
		/// </summary>
		public long receiveTime { get; set; }

		/// <summary>
		/// offset of this command in this server
		/// </summary>
		public long receiveOffset { get; set; }
	}
}
