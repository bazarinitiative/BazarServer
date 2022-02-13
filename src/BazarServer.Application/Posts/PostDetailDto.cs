namespace BazarServer.Application.Posts
{

	public class PostDetailDto
	{
		public PostDto? current { get; set; }

		/// <summary>
		/// maybe null if current is topmost
		/// </summary>
		public PostDto? parent { get; set; }
		/// <summary>
		/// maybe null if current is topmost or parrent is topmost
		/// </summary>
		public PostDto? thread { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public List<PostDto> replies { get; set; } = new List<PostDto>();
	}

}
