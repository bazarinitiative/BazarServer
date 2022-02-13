using BazarServer.Entity.SeedWork;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// statistic of user
	/// </summary>
	public class UserStatistic : IStoreData
	{
		/// <summary>
		/// 
		/// </summary>
		public string userID { get; set; }

		/// <summary>
		/// count of posts
		/// </summary>
		public int postCount { get; set; }

		/// <summary>
		/// count of liked (of posts)
		/// </summary>
		public int likedCount { get; set; }

		/// <summary>
		/// count of following
		/// </summary>
		public int followingCount { get; set; }

		/// <summary>
		/// count of be followed
		/// </summary>
		public int followedCount { get; set; }

		public UserStatistic(string userID)
		{
			this.userID = userID;
		}
	}
}
