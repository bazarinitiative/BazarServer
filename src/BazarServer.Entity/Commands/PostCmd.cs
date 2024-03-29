﻿using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	public class PostCmd : ICommandContent
	{
		/// <summary>
		/// unique ID of a post. reply/repost are also post. 
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string postID { get; set; } = "";

		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }
		public string commandType { get; set; } = "";

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
		/// Word between '#' and first non-letter-nor-digit char will be treat as a hashtag declaration.
		/// </summary>
		[StringLength(300)]
		public string content { get; set; } = "";

		/// <summary>
		/// content language like 'en-US', 'de', 'ja', 'zh-CN'. as a reference value to help language auto detect.
		/// </summary>
		[StringLength(20)]
		public string contentLang { get; set; } = "";
	}
}