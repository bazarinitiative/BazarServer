using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace Common.Utils
{
	public static class ConfigHelper
	{
		/// <summary>
		/// try config file, environment and AppSettings.
		/// because azure configuration environment variable is special.
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetConfigValue(IConfiguration? configuration, string key)
		{
			var ss = configuration?[key];
			if (string.IsNullOrEmpty(ss))
			{
				ss = Environment.GetEnvironmentVariable(key);
			}
			if (string.IsNullOrEmpty(ss))
			{
				ss = ConfigurationManager.AppSettings[key].ToStringSupportNull();
			}
			return ss;
		}
	}
}
