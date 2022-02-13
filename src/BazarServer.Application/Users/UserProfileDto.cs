using BazarServer.Entity.Storage;

namespace BazarServer.Application.Users
{
	public class UserProfileDto
	{
		public UserInfo userInfo { get; set; }
		public UserStatistic userStatistic { get; set; }

		public UserProfileDto(UserInfo userInfo, UserStatistic userStatistic)
		{
			this.userInfo = userInfo;
			this.userStatistic = userStatistic;
		}

	}
}
