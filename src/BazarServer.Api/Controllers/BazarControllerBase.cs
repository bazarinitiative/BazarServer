using BazarServer.Entity.SeedWork;
using MediatR;

namespace BazarServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BazarControllerBase : ControllerBase
{
	protected ApiResponse Error(string msg)
	{
		return new ApiResponse(false, msg);
	}

	protected ApiResponse Success()
	{
		return new ApiResponse();
	}

	protected ApiResponse<T> Error<T>(string msg, T? result = default(T))
	{
		return new ApiResponse<T>(false, msg, result);
	}

	protected ApiResponse<T> Success<T>(T? result = default(T))
	{
		return new ApiResponse<T>(true, "", result);
	}

}

