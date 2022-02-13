using BazarServer.Entity.Storage;

namespace BazarServer.Application.Users
{
	/// <summary>
	/// we don't return userPic directly for bandwidth consideration, there should be client side cache.
	/// </summary>
	public class UserDto
	{
		public UserDto(string userID)
		{
			this.userID = userID;
		}

		public string userID { get; set; } = "";

		/// <summary>
		/// 
		/// </summary>
		public UserInfo userInfo { get; set; } = new UserInfo();

		//public UserPic userPic { get; set; } = new UserPic("");

		/// <summary>
		/// 
		/// </summary>
		public UserStatistic userStatistic { get; set; } = new UserStatistic("");
	}
}
