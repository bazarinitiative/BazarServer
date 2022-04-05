using BazarServer.Entity.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	public class NotifyMessage : IStoreData
	{
		/// <summary>
		/// uniqueID of this notification.
		/// </summary>
		[StringLength(30)]
		public string notifyID { get; set; } = "";

		/// <summary>
		/// who will receive this notify
		/// </summary>
		public string userID { get; set; } = "";

		/// <summary>
		/// in milli seconds
		/// </summary>
		public long notifyTime { get; set; } = default;

		/// <summary>
		/// including "Like", "Reply", "Mention", "Follow", "AddList", etc
		/// like is from user to post.
		/// mention is from current post, to related inherit post or empty
		/// </summary>
		public string notifyType { get; set; } = "";

		/// <summary>
		/// whose action cause this notification
		/// </summary>
		public string fromWho { get; set; } = "";

		/// <summary>
		/// postID if type = 'Like'
		/// postID if type = 'Reply'
		/// postID if type = 'Mention'
		/// empty if type = 'Follow'
		/// listID if type = 'AddList'
		/// </summary>
		public string fromWhere { get; set; } = "";
	}
}
