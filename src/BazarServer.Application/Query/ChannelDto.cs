using BazarServer.Entity.Storage;

namespace BazarServer.Application.Query
{
	public class ChannelDto
	{
		public Channel channel { get; set; }
		public int memberCount { get; set; }
		public int followerCount { get; set; }

		public ChannelDto(Channel channel)
		{
			this.channel = channel;
		}
	}
}
