using BazarServer.Entity.Storage;

namespace BazarServer.Application.Commands
{
	public interface ICommandManager
	{
		/// <summary>
		/// after command level verfication, save command and dispatch command to different processor
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="commandFrom">where is this command from, server BaseUrl or client IP</param>
		/// <returns></returns>
		Task<MdtResp> SaveAndDispatch(UserCommand cmd, string commandFrom);
	}
}
