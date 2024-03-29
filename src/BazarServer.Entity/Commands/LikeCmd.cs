﻿using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// userID+postID is unique
	/// </summary>
	public class LikeCmd : ICommandContent
	{
		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }
		public string commandType { get; set; } = "";

		/// <summary>
		/// 
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string postID { get; set; } = "";
	}
}
