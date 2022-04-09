using BazarServer.Entity.Storage;

namespace BazarServer.Application.Query
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
