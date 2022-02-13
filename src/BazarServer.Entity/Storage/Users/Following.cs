using BazarServer.Entity.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// userID+targetID is unique.
	/// targetType is a describe info of targetID.
	/// </summary>
	public class Following : IStoreData
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
