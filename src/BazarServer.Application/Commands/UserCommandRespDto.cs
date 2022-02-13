namespace BazarServer.Application.Commands
{
	/// <summary>
	/// response data for a command. usually lack of data. client should upload the necessary data and retry
	/// </summary>
	public class UserCommandRespDto
	{
		/// <summary>
		/// need some extra UserInfo to execute a user command such as 'Following', 'Like', 'Repost', etc.
		/// </summary>
		public List<string> lackUserInfo { get; set; } = new List<string>();

		/// <summary>
		/// need some extra Post to execute a user command.
		/// </summary>
		public List<string> lackPost { get; set; } = new List<string>();

		public List<string> lackChannel { get; set; } = new List<string>();

		public void AddUser(string userID)
		{
			lackUserInfo.Add(userID);
		}

		public void AddPost(string postID)
		{
			lackPost.Add(postID);
		}

		public void AddChannel(string channelID)
		{
			lackChannel.Add(channelID);
		}
	}
}
