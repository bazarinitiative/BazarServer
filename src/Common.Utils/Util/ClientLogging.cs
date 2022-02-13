using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Common.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class ClientLogging
	{
		public static void SetupLogging(bool ui, string logPath)
		{
			FileIO.EnsureDirectory(logPath);

			if (ui)
			{
				//LoggerFactory.AddProvider(new CommonConsoleLoggerProvider());
				LoggerFactory.AddProvider(new CommonFileLoggerProvider($"{logPath}\\Common-Desktop.ui.log", "UI"));
			}
			else
			{
				LoggerFactory.AddProvider(new CommonFileLoggerProvider($"{logPath}\\Common-Desktop.bg.log", "BG"));
			}
		}

		public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

		public static ILogger CreateLogger<T>()
		{
			return LoggerFactory.CreateLogger<T>();
		}

	}

	class CommonConsoleLogger : ILogger
	{
		private readonly string ClassName;
		public CommonConsoleLogger(string categoryName)
		{
			ClassName = categoryName;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			Debug.WriteLine(string.Format("{0:s} [{1}] [{2}] ", DateTime.UtcNow, logLevel, ClassName) + formatter(state, exception));
		}
	}

	class CommonConsoleLoggerProvider : ILoggerProvider
	{
		public ILogger CreateLogger(string categoryName)
		{
			return new CommonConsoleLogger(categoryName);
		}

		public void Dispose()
		{

		}
	}

	class CommonFileLogger : ILogger
	{
		private readonly string ClassName;
		private readonly CommonFileLoggerProvider Provider;

		public CommonFileLogger(string categoryName, CommonFileLoggerProvider provider)
		{
			ClassName = categoryName;
			Provider = provider;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
		{
			var tail = "";
			if (exception != null)
			{
				tail = $"\n\tException.Message: {exception.ToString().Left(400)}";
			}
			Provider.Log($"[{logLevel}] [{ClassName}] - {state}{tail}");
		}
	}

	public class CommonFileLoggerProvider : ILoggerProvider
	{
		private readonly string Filename;
		private readonly string OldFilename;
		private readonly string Prefix;
		private const int MaxLogSize = 256 * 1024;
		private static readonly object Lock = new object();

		public CommonFileLoggerProvider(string filename, string prefix)
		{
			Filename = filename;
			OldFilename = Filename + ".old";
			Prefix = prefix;
		}

		public void TruncateLog()
		{
			try
			{
				var length = new FileInfo(Filename).Length;
				if (length > MaxLogSize)
				{
					if (File.Exists(OldFilename))
					{
						File.Delete(OldFilename);
					}
					File.Move(Filename, OldFilename);
					File.AppendAllText(Filename, $"{DateTime.UtcNow.ToString("s")} [CommonFileLoggerProvider] truncated log file\n");
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("CommonFileLoggerProvider failed to truncate file: {0}", e));
			}
		}

		public void Log(string line)
		{
			lock (Lock)
			{
				try
				{
					TruncateLog();
					File.AppendAllText(Filename, $"{DateTime.UtcNow.ToString("s")} [{Prefix}] {line}\n");
				}
				catch (Exception e)
				{
					Debug.WriteLine(string.Format("CommonFileLoggerProvider failed to write: {0}", e));
				}
			}
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new CommonFileLogger(categoryName, this);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

	}
}