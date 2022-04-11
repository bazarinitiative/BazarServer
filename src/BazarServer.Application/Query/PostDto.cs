using BazarServer.Entity.Storage;

namespace BazarServer.Application.Query
{
	public class PostDto
	{
		public Post post { get; set; }
		public PostStatistic ps { get; set; }
		/// <summary>
		/// empty means not liked
		/// </summary>
		public bool liked { get; }

		/// <summary>
		/// userID of post.replyTo. empty if not reply to anyone
		/// </summary>
		public string replyToUser { get; }

		/// <summary>
		/// 
		/// </summary>
		public bool bookmarked { get; }

		public PostDto(Post post, PostStatistic ps, bool liked, string replyToUser, bool bookmarked)
		{
			this.post = post;
			this.ps = ps;
			this.liked = liked;
			this.replyToUser = replyToUser;
			this.bookmarked = bookmarked;
		}

	}
}
