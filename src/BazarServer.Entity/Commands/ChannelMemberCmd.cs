using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// 
	/// </summary>
	public class ChannelMemberCmd : ICommandContent
	{
		/// <summary>
		/// uniqueID of this Channel
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string channelID { get; set; } = "";

		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }
	}
}
