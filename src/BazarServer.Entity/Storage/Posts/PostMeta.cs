namespace BazarServer.Entity.Storage
{
	public class PostMeta
	{
		public string postID { get; set; } = "";

		/// <summary>
		/// when create-command is received.
		/// </summary>
		public DateTime receiveCreate { get; set; } = DateTime.Now;

		/// <summary>
		/// server BaseUrl or client IP, where this create-command receive from.
		/// </summary>
		public string createFrom { get; set; } = "";

		/// <summary>
		/// when delete-command is received
		/// </summary>
		public DateTime receiveDelete { get; set; }

		/// <summary>
		/// server BaseUrl or client IP, where this delete-command receive from.
		/// </summary>
		public string deleteFrom { get; set; } = "";

		/// <summary>
		/// parent postID, grand-parent postID, etc. separated by ',' end without ','
		/// </summary>
		public string inheritPosts { get; set; } = "";
	}
}
