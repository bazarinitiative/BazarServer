using Common.Utils.Util;

namespace BazarServer;

public static class AspNetExtention
{
	/// <summary>
	/// get full url like: https://www.bing.com/search?q=bazar
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	public static string GetFullUrl(this HttpRequest request)
	{
		if (request.QueryString.HasValue)
		{
			var full = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
			return full;
		}
		else
		{
			var full = $"{request.Scheme}://{request.Host}{request.Path}";
			return full;
		}
	}

	public static string GetRemoteIP(this HttpContext context)
	{
		return context.Connection.RemoteIpAddress.ToStringSupportNull();
	}

	/// <summary>
	/// for server after local nginx, get client real ip
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public static string GetRealIP(this HttpContext context)
	{
		var remote = context.GetRemoteIP();
		var xreal = context.Request.Headers["X-Real-IP"];

		var ip = remote;
		if (NetHelper.IsInternalIP(remote) && !string.IsNullOrEmpty(xreal))
		{
			ip = xreal;
		}
		return ip;
	}
}
