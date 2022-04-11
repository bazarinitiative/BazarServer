using BazarServer.Entity.Storage;

namespace BazarServer.Application.Query
{
	public class ChannelMemberDto
	{
		public ChannelMember channelMember { get; set; }

		public ChannelMemberDto(ChannelMember channelMember)
		{
			this.channelMember = channelMember;
		}
	}
}
