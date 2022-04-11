using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	public class Bookmark : IStoreData
	{
		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string postID { get; set; }

		public Bookmark(string postID)
		{
			this.postID = postID;
		}

	}
}
