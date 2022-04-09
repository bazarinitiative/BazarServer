using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// user avatar pic
	/// </summary>
	public class UserPic : IStoreData
	{
		public string userID { get; set; }

		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// base64 encoded user picture. string length 50KB at most.
		/// </summary>
		[StringLength(50 * 1024)]
		public string pic { get; set; } = "";

		public UserPic(string userID)
		{
			this.userID = userID;
		}

	}
}
