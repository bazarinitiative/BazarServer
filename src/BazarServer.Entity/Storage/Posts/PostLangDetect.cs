using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BazarServer.Entity.Storage
{
	public class PostLangDetect : IStoreData
	{
		public string postID { get; set; } = "";

		public DateTime detectTime { get; set; } = DateTime.Now;

		public int detectLength = 20;

		public string detectResult { get; set; } = "";
	}
}
