using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// one bazar user
	/// </summary>
	public class UserInfoCmd : ICommandContent
	{
		[StringLength(30, MinimumLength = 30)]
		public string userID { get; set; } = "";

		[StringLength(30, MinimumLength = 30)]
		public string commandID { get; set; } = "";

		public long commandTime { get; set; }

		/// <summary>
		/// publicKey that everyone can see
		/// </summary>
		[StringLength(300)]
		public string publicKey { get; set; } = "";

		/// <summary>
		/// user can set a name to display
		/// </summary>
		[StringLength(50)]
		public string userName { get; set; } = "";

		public bool bot { get; set; } = false;

		/// <summary>
		/// biography of this user
		/// </summary>
		[StringLength(300)]
		public string biography { get; set; } = "";

		/// <summary>
		/// location
		/// </summary>
		[StringLength(100)]
		public string location { get; set; } = "";

		/// <summary>
		/// website
		/// </summary>
		[StringLength(100)]
		public string website { get; set; } = "";

	}
}
