using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// userID+targetID is unique.
	/// targetType is a describe info of targetID.
	/// </summary>
	public class FollowingCmd : ICommandContent
	{
		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// "User" or "Channel"
		/// </summary>
		[StringLength(20)]
		public string targetType { get; set; } = "";

		[StringLength(30, MinimumLength = 30)]
		public string targetID { get; set; } = "";
	}
}
