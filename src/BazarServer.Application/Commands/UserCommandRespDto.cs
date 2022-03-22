namespace BazarServer.Application.Commands
{
	public enum CommandErrorCode
	{
		/// <summary>
		/// 1-99 reserved
		/// </summary>
		Default = 0,
		NoUser = 100,
		NoPost = 101,
		NoChannel = 102,
		NoFollowing = 103,
	}

	/// <summary>
	/// response data for a command. usually lack of data. client should upload the necessary data and retry
	/// </summary>
	public class UserCommandRespDto
	{
		public CommandErrorCode code { get; set; }

		/// <summary>
		/// content depends on CommandErrorCode. usually the id of lack-resource
		/// </summary>
		public string message { get; set; } = "";

		public UserCommandRespDto()
		{
		}

		public UserCommandRespDto(CommandErrorCode code, string message)
		{
			this.code = code;
			this.message = message;
		}
	}
}
