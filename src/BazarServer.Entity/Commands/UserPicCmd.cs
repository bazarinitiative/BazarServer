using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// user avatar pic
	/// </summary>
	public class UserPicCmd : ICommandContent
	{
		public string userID { get; set; }

		public string commandID { get; set; } = "";
		public long commandTime { get; set; }
		public string commandType { get; set; } = "";

		/// <summary>
		/// base64 encoded user picture. string length 50KB at most.
		/// </summary>
		[StringLength(50 * 1024)]
		public string pic { get; set; } = "";

		public UserPicCmd(string userID)
		{
			this.userID = userID;
		}

	}
}
