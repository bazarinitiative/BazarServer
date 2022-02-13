using BazarServer.Entity.SeedWork;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// statistic of post
	/// </summary>
	public class PostStatistic : IStoreData
	{
		/// <summary>
		/// 
		/// </summary>
		public string postID { get; set; }

		/// <summary>
		/// count of replies
		/// </summary>
		public int replyCount { get; set; }

		/// <summary>
		/// count of reposts
		/// </summary>
		public int repostCount { get; set; }

		/// <summary>
		/// count of likes
		/// </summary>
		public int likeCount { get; set; }

		public PostStatistic(string postID)
		{
			this.postID = postID;
		}

	}
}
