namespace BazarServer;

/// <summary>
/// request rate limit, reqest info log, etc.
/// </summary>
internal class MyMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<MyMiddleware> _logger;

	public MyMiddleware(RequestDelegate next, ILogger<MyMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{

			if (!RateLimit(context))
			{
				_logger.LogError("RateLimit check fail");

				await context.Response.WriteAsync("RateLimit error");
				context.Abort();
				return;
			}

			if (context.Request.Path == "/")
			{
				await context.Response.WriteAsync($"BazarServer at {DateTime.UtcNow}");
				return;
			}

			var cultureQuery = context.Request.Query["culture"];
			if (!string.IsNullOrWhiteSpace(cultureQuery))
			{
				context.Items["culture"] = cultureQuery;
			}

			string fullurl = context.Request.GetFullUrl();
			var requestbody = await ReadRequestBody(context.Request);
			var info = $"{fullurl} {context.Request.Method}: {requestbody.Left(2000)}";

			// Call the next delegate/middleware in the pipeline
			await _next(context);

			_logger.LogInformation($"{info}, result={context.Response.StatusCode}");
			if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
			{
				_logger.LogError($"{info}, errorCode={context.Response.StatusCode}");
			}

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "invoke filter error");

			throw;
		}

	}

	static FrequencyControl fcIP = new FrequencyControl(10, 100);

	private bool RateLimit(HttpContext context)
	{
		var ip = context.GetRealIP();

		if (!fcIP.Check(ip))
		{
			return false;
		}

		return true;
	}

	private static async Task<string> ReadRequestBody(HttpRequest request)
	{
		request.EnableBuffering();
		var bodyAsText = await new StreamReader(request.Body).ReadToEndAsync();
		request.Body.Position = 0;
		return bodyAsText;
	}
}
