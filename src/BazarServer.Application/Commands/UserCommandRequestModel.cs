using BazarServer.Entity.Storage;
using Common.Utils;
using System.ComponentModel.DataAnnotations;

namespace BazarServer.Application.Commands
{
	public class UserCommandRequestModel
	{
		/// <summary>
		/// uniqueID of this command
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string commandID { get; set; } = "";

		/// <summary>
		/// milliseconds since EPOCH
		/// </summary>
		public long commandTime { get; set; }

		/// <summary>
		/// who initiative this command
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string userID { get; set; } = "";

		/// <summary>
		/// Post, Following, Repost, Like, Delete, etc...
		/// </summary>
		[StringLength(20)]
		public string commandType { get; set; } = "";

		/// <summary>
		/// added since v0.2
		/// </summary>
		[StringLength(10)]
		public string version { get; set; } = "";

		/// <summary>
		/// a json string of this command detail content
		/// </summary>
		[StringLength(50 * 1024)]
		public string commandContent { get; set; } = "";

		/// <summary>
		/// signature of commandContent.
		/// signed by user with privateKey. can be verified by user publicKey.
		/// </summary>
		[StringLength(200)]
		public string signature { get; set; } = "";

		public UserCommand ToUserCommand()
		{
			var req = this;
			return new UserCommand()
			{
				commandID = req.commandID,
				commandTime = req.commandTime,
				userID = req.userID,
				commandType = req.commandType,
				commandContent = req.commandContent,
				signature = req.signature,
				receiveTime = DateHelper.CurrentTimeMillis()
			};
		}

		public bool IsValidContentLength()
		{
			int maxLen = 1024;
			if (commandType == "UserPic")
			{
				maxLen = 55 * 1024;
			}
			if (commandType == "UserInfo")
			{
				maxLen = 2 * 1024;
			}
			return commandContent.Length <= maxLen;
		}
	}
}
