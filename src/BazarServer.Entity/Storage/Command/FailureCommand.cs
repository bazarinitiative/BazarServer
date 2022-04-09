
namespace BazarServer.Entity.Storage
{
	public class FailureCommand : IStoreData
	{
		/// <summary>
		/// 
		/// </summary>
		public string commandID { get; set; } = "";

		/// <summary>
		/// 
		/// </summary>
		public UserCommand? command { get; set; }

		/// <summary>
		/// timeMillis
		/// </summary>
		public long failTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string errMsg { get; set; } = "";
	}
}
