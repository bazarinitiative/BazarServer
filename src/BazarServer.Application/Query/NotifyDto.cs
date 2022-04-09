using BazarServer.Entity.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BazarServer.Application.Query
{
	public class NotifyDto
	{
		public NotifyMessage noti { get; set; }
		public PostDto postDto { get; set; }
		public bool isDirectReplyTo { get; set; }

		public NotifyDto(NotifyMessage noti, PostDto post, bool isDirectReplyTo)
		{
			this.noti = noti;
			this.postDto = post;
			this.isDirectReplyTo = isDirectReplyTo;
		}
	}
}
