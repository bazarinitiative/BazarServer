using BazarServer.Entity.SeedWork;
using BazarServer.Entity.Storage;
using Common.Utils;
using MediatR;

namespace BazarServer.Application.Commands
{
	/// <summary>
	/// official process entrance of all ICommandContent, include DeleteCmd.
	/// ICommandContent may come from webapi controller, may come from peer server.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MdtRequest<T> : IRequest<MdtResp> where T : ICommandContent
	{
		public string commandID { get; }
		public string userID { get; }
		/// <summary>
		/// milli seconds since epoch
		/// </summary>
		public long commandTime { get; }
		public T model { get; }

		/// <summary>
		/// where is this command from, server BaseUrl or client IP
		/// </summary>
		public string commandFrom { get; }

		public MdtRequest(string commandID, string userID, long commandTime, T model, string commandFrom)
		{
			this.commandID = commandID;
			this.userID = userID;
			this.commandTime = commandTime;
			this.model = model;
			this.commandFrom = commandFrom;
		}

		internal (bool success, string msg) Validate()
		{
			if (!commandID.IsLetterDigit30())
			{
				return (false, "invalid commandID");
			}
			if (!userID.IsLetterDigit30())
			{
				return (false, "invalid userID");
			}

			if (commandTime - DateHelper.CurrentTimeMillis() > 60 * 1000)
			{
				return (false, "wrong time");
			}

			if (commandID != model.commandID) return (false, "commandID not match");
			if (commandTime != model.commandTime) return (false, "commandTime not match");
			if (userID != model.userID) return (false, "userID not match");
			return (true, "");
		}

		/// <summary>
		/// will validate MdtRequest for commandID, commandTime, userID.
		/// will validate req.content model.
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <param name="req"></param>
		/// <param name="commandFrom">where is this command from, server BaseUrl or client IP</param>
		/// <returns></returns>
		public static (MdtRequest<T2>? md, string msg) FromCommand<T2>(UserCommand req, string commandFrom) where T2 : ICommandContent
		{
			var model = Json.Deserialize<T2>(req.commandContent, true);
			if (model == null)
			{
				return (null, $"fail to Deserialize: {req.commandContent}");
			}

			if (!ValidationHelper.Validate(model, out var results))
			{
				return (null, $"validate fail: {results.FirstOrDefault()}");
			}

			MdtRequest<T2> ret = new MdtRequest<T2>(req.commandID, req.userID, req.commandTime, model, commandFrom);

			var valid = ret.Validate();
			if (!valid.success)
			{
				return (null, valid.msg);
			}

			return (ret, "");
		}
	}
}
