using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// This is asking every related server to delete UGC, physically or logically.
	/// No one can ensure a deletion once published, including centralized platform, because third party would snapshot or record.
	/// </summary>
	public class DeleteCmd : ICommandContent
	{
		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// Post, Repost, Like, Following, etc
		/// </summary>
		[StringLength(20)]
		public string deleteType { get; set; } = "";

		/// <summary>
		/// modelID of target
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string targetID { get; set; } = "";
	}
}
