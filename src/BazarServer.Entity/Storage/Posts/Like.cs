using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// userID+postID is unique
	/// </summary>
	public class Like : IStoreData
	{
		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string postID { get; set; } = "";
	}
}
