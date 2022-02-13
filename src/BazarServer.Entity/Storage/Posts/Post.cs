using BazarServer.Entity.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	public class Post : IStoreData
	{
		/// <summary>
		/// unique ID of a post. reply/repost are also post. 
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string postID { get; set; } = "";

		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// postID of the original post
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string threadID { get; set; } = "";

		/// <summary>
		/// postID of which we reply to. we can reply to an original post or a reply
		/// </summary>
		[StringLength(30, MinimumLength = 0)]
		public string replyTo { get; set; } = "";

		/// <summary>
		/// 
		/// </summary>
		public bool isRepost { get; set; } = false;

		/// <summary>
		/// content of a post. RTF 1.7 standard.
		/// Word between '#' and first non-letter-nor-digit char will be treat as a tag declaration.
		/// </summary>
		[StringLength(300)]
		public string content { get; set; } = "";

		/// <summary>
		/// content language like 'en-US', 'de', 'ja', 'zh-CN'. as a reference value to help language auto detect.
		/// </summary>
		[StringLength(20)]
		public string contentLang { get; set; } = "";

		/// <summary>
		/// if this field is not empty, keep content/media be folded until user click 'expand'
		/// </summary>
		[StringLength(100)]
		public string foldingText { get; set; } = "";

		/// <summary>
		/// image, video, gif, etc...
		/// </summary>
		[StringLength(10)]
		public string mediaType { get; set; } = "";

		/// <summary>
		/// 1 video/gif or at most 4 images. split by char \x001
		/// </summary>
		[StringLength(400)]
		public string mediaUrls { get; set; } = "";

		/// <summary>
		/// deleted posts need to stay in system, for display purpose.
		/// </summary>
		public bool deleted { get; set; } = false;
	}
}