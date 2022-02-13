global using Common.Utils;
global using Microsoft.AspNetCore.Mvc;
using NLog.Web;
using System.Diagnostics;

namespace BazarServer;

public class Program
{
	public static void Main(string[] args)
	{
		try
		{
			Console.WriteLine("BazarServer start");
			var ret = CheckEnv();
			if (!ret.success)
			{
				Console.WriteLine(ret.msg);
				return;
			}

			ConfigNLog();

			var host = CreateHostBuilder(args).Build();
			host.Run();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());

			var title = "app start fail";

			MailHelper.ReportMail(title, ex);
		}
	}

	private static (bool success, string msg) CheckEnv()
	{
		string[] ay = {
			"BazarMail",
			"BazarMongodb",
			"BazarBaseUrl"
		};
		var ret = CheckEnv(ay.ToList());
		return ret;
	}

	private static (bool success, string msg) CheckEnv(List<string> ayNames)
	{
		foreach (var name in ayNames)
		{
			var str = ConfigHelper.GetConfigValue(null, name);
			if (string.IsNullOrEmpty(str))
			{
				return (false, $"EnvironmentVariable [{name}] does not exist or empty");
			}
		}
		return (true, "ok");
	}

	public static IHostBuilder CreateHostBuilder(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args)
					.ConfigureServices((hostContext, services) =>
					{
					})
					.ConfigureWebHostDefaults(webBuilder =>
					{
						webBuilder.UseStartup<Startup>();
					})
					.ConfigureAppConfiguration(x => x.AddEnvironmentVariables())
					.UseNLog();
		return builder;
	}

	[Conditional("DEBUG")]
	private static void ConfigNLog()
	{
		//NLogBuilder.ConfigureNLog("nlog.debug.config");
	}
}
