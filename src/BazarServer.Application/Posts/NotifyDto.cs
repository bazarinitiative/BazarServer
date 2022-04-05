using BazarServer.Entity.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BazarServer.Application.Posts
{
	public class NotifyDto
	{
		public NotifyMessage noti { get; set; }
		public Post post { get; set; }
		public bool isDirectReplyTo { get; set; }

		public NotifyDto(NotifyMessage noti, Post post, bool isDirectReplyTo)
		{
			this.noti = noti;
			this.post = post;
			this.isDirectReplyTo = isDirectReplyTo;
		}
	}
}
