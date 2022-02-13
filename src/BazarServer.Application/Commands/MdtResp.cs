namespace BazarServer.Application.Commands
{
	public class MdtResp
	{
		public bool success { get; set; }
		public string msg { get; set; }
		public UserCommandRespDto resp { get; set; }

		public MdtResp(bool success, string msg)
		{
			this.success = success;
			this.msg = msg;
			resp = new UserCommandRespDto();
		}

		public MdtResp(bool success, string msg, UserCommandRespDto rc)
		{
			this.success = success;
			this.msg = msg;
			resp = rc;
		}

		public MdtResp((bool success, string msg) val)
		{
			this.success = val.success;
			this.msg = val.msg;
			resp = new UserCommandRespDto();
		}
	}
}
