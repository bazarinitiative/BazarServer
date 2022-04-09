using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BazarServer.Entity.Storage
{
	public class NotifyGetTime : IStoreData
	{
		/// <summary>
		/// 
		/// </summary>
		public string userID { get; set; } = "";
		/// <summary>
		/// timeMillis
		/// </summary>
		public long getTime { get; set; }
	}
}
