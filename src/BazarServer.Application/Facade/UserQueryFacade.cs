using BazarServer.Entity.Storage;
using Common.Utils;

namespace BazarServer.Application.Users
{
	public class UserQueryFacade
	{
		/// <summary>
		/// for some heavy or private query, we want to make sure the qurey comes from specific user
		/// </summary>
		/// <param name="userRepository"></param>
		/// <param name="userID"></param>
		/// <param name="queryTimestamp">time milli</param>
		/// <param name="queryToken"></param>
		/// <returns></returns>
		public static async Task<(bool success, string msg)> CheckQuery(IUserRepository userRepository,
																  string userID,
																  long queryTimestamp,
																  string queryToken)
		{
			var user = await userRepository.GetUserInfoAsync(userID);
			if (user == null)
			{
				return (false, $"user not found");
			}
			var diff = Math.Abs(queryTimestamp - DateHelper.CurrentTimeMillis());
			if (diff > 600 * 1000)
			{
				return (false, "time not match");
			}
			if (!Encryption.CheckSignature(queryTimestamp.ToString(), queryToken, user.publicKey))
			{
				return (false, "signature not match");
			}
			return (true, "ok");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userRepository"></param>
		/// <param name="userID"></param>
		/// <param name="cacheMilli">millseconds that result will cache</param>
		/// <returns></returns>
		public static async Task<UserDto> GetUserDto_WithCache(IUserRepository userRepository, string userID, int cacheMilli = 1000)
		{
			var key = $"GetUserDto_{userID}";
			var ret = await CacheHelper.WithCacheAsync(
				key,
				async () => await GetUserDtoInt(userRepository, userID),
				cacheMilli,
				false);
			if (ret == null)
			{
				ret = new UserDto(userID);
			}
			return ret;
		}

		private static async Task<UserDto> GetUserDtoInt(IUserRepository userRepository, string userID)
		{
			return new UserDto(userID)
			{
				userInfo = await userRepository.GetUserInfoAsync(userID) ?? new UserInfo(),
				userStatistic = await userRepository.GetUserStatisticAsync(userID) ?? new UserStatistic(userID),
			};
		}
	}
}
