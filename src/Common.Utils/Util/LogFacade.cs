using NLog;

namespace Common.Utils
{
	public static class LogFacade
	{
		static ILogger _logger;

		static LogFacade()
		{
			_logger = new NLog.LogFactory().GetLogger("LogFacade", typeof(LogFacade));
		}

		public static void LogInformation(string msg)
		{
			_logger.Log(LogLevel.Info, msg);
		}

		public static void LogError(string msg)
		{
			_logger.Log(LogLevel.Error, msg);
		}

		public static void LogError(Exception ex, string msg)
		{
			_logger.Log(LogLevel.Error, ex, msg);
		}

	}
}
