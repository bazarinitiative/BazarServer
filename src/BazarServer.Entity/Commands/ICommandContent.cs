using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// Indicate this is a UserCommand, need signature, serialize and storage.
	/// </summary>
	public interface ICommandContent
	{
		/// <summary>
		/// who send out the commandID. who own the data.
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string userID { get; set; }

		/// <summary>
		/// uniqueID of a userCommand. randstring(30)
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string commandID { get; set; }

		/// <summary>
		/// in milli seconds. user declare commandTime may not be true
		/// </summary>
		public long commandTime { get; set; }
	}
}
