﻿using System.ComponentModel.DataAnnotations;

namespace BazarServer.Entity.Storage
{
	/// <summary>
	/// 
	/// </summary>
	public class ChannelMember : IStoreData
	{
		/// <summary>
		/// who send this command
		/// </summary>
		public string userID { get; set; } = "";
		public string commandID { get; set; } = "";
		public long commandTime { get; set; }

		/// <summary>
		/// uniqueID of this channel-member relationship
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string cmID { get; set; } = "";


		/// <summary>
		/// uniqueID of this Channel
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string channelID { get; set; } = "";

		/// <summary>
		/// whom to add to this channel
		/// </summary>
		[StringLength(30, MinimumLength = 30)]
		public string memberID { get; set; } = "";
	}
}
