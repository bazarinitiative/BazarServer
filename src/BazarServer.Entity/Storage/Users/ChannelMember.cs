using BazarServer.Entity.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// 
	/// </summary>
	public class ChannelMember : IStoreData
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
