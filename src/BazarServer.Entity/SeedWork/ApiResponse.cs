
namespace BazarServer.Entity.SeedWork
{
	public class ApiResponse<T>
	{
		public ApiResponse()
			: this(true, string.Empty)
		{
		}

		public ApiResponse(bool success, string msg)
			: this(success, msg, default(T))
		{
		}

		public ApiResponse(bool success, string msg, T? data)
		{
			this.success = success;
			this.msg = msg;
			this.data = data;
		}

		/// <summary>
		/// success or not
		/// </summary>
		public bool success { get; set; }

		/// <summary>
		/// variant message may change later.
		/// </summary>
		public string msg { get; set; }

		/// <summary>
		/// response data, maybe null on failure.
		/// </summary>
		public T? data { get; set; }
	}

	public class ApiResponse : ApiResponse<object>
	{
		public ApiResponse() : base()
		{
		}

		public ApiResponse(bool success, string msg) : base(success, msg)
		{
		}
	}
}
