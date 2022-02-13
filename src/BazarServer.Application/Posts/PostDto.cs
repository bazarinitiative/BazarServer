using BazarServer.Entity.Storage;

namespace BazarServer.Application.Posts
{
	public class PostDto
	{
		public Post post { get; set; }
		public PostStatistic ps { get; set; }
		/// <summary>
		/// empty means not liked
		/// </summary>
		public bool liked { get; }

		public PostDto(Post post, PostStatistic ps, bool liked)
		{
			this.post = post;
			this.ps = ps;
			this.liked = liked;
		}

	}
}
