
using BazarServer.Entity.Storage;

namespace BazarServer.Application.Query
{
	public class BookmarkDto
	{
		public Bookmark bookmark { get; set; }
		public PostDto post { get; set; }

		public BookmarkDto(Bookmark bookmark, PostDto post)
		{
			this.post = post;
			this.bookmark = bookmark;
		}
	}
}
