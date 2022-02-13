using BazarServer.Entity.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// group of users. like channel in telegram or list in twitter. 
	/// </summary>
	public class Channel : IStoreData
	{
		/// <summary>
		/// uniqueID of this Channel
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string channelID { get; set; }

		public string userID { get; set; }
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(300)]
		public string description { get; set; } = "";

		public Channel(string channelID, string userID)
		{
			this.channelID = channelID;
			this.userID = userID;
		}
	}
}
