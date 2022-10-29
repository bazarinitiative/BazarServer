﻿using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Commands
{
	/// <summary>
	/// group of users. like channel in telegram or list in twitter. 
	/// </summary>
	public class ChannelCmd : ICommandContent
	{
		public string userID { get; set; }
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }
		public string commandType { get; set; } = "";

		/// <summary>
		/// uniqueID of this Channel
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string channelID { get; set; }

		/// <summary>
		/// displayName of this Channel
		/// </summary>
		[StringLength(100)]
		public string channelName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(300)]
		public string description { get; set; } = "";

		public ChannelCmd(string channelID, string userID, string channelName)
		{
			this.channelID = channelID;
			this.userID = userID;
			this.channelName = channelName;
		}
	}
}
