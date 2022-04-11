using BazarServer.Entity.Storage;

namespace BazarServer.Infrastructure.Repository
{
	public interface IUserRepository
	{
		Task<UserInfo?> GetUserInfoAsync(string userID);
		/// <summary>
		/// null will not cache
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="cacheMilli"></param>
		/// <returns></returns>
		Task<UserInfo?> GetUserInfoAsync_WithCache(string userID, int cacheMilli = 1000);
		Task<UserPic?> GetUserPicAsync(string userID);
		Task<UserStatistic?> GetUserStatisticAsync(string userID);
		Task SaveUserAsync(UserInfo model);
		Task UpsertUserStatisticAsync(string userID, int addPostCount = 0, int addLikedCount = 0, int addFollowingCount = 0, int addFollowedCount = 0);
		Task<bool> IsExistUserAsync(string userID);

		Task<List<UserInfo>> GetRandomUser(int count);

		/// <summary>
		/// get the list of followers
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		Task<List<Following>> GetUserFollowers(string userID, int page, int pageSize);

		/// <summary>
		/// get the list of followees
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		Task<List<Following>> GetUserFollowees(string userID, int page, int pageSize);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ay"></param>
		/// <param name="startIdx"></param>
		/// <param name="endIdx"></param>
		/// <returns></returns>
		Task<List<UserInfo>> Search(List<string> ay, int startIdx, int endIdx);

		/// <summary>
		/// get the record of one following
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="targetID">userID or channelID</param>
		Task<Following?> GetUserFollowing(string userID, string targetID);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="notifyMessage"></param>
		/// <returns></returns>
		Task AddUserNotify(NotifyMessage notifyMessage);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="startTime">startTime (include) of backwards</param>
		/// <param name="backwardCount"></param>
		/// <returns></returns>
		Task<List<NotifyMessage>> GetUserNotify(string userID, long startTime, int backwardCount = 1000);

		/// <summary>
		/// update NotifyGetTime if bigger
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="getTime"></param>
		/// <returns></returns>
		Task TryUpdateNotifyGetTime(string userID, long getTime);

		/// <summary>
		/// get notify count since last NotifyGetTime
		/// </summary>
		/// <returns></returns>
		Task<long> GetNewNotifyCount(string userID);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		Task<List<Like>> GetUserLikes(string userID, int page, int pageSize);
		Task RemoveNotify(string fromWhere);
		Task<List<Channel>> getUserChannels(string userID);
		Task<List<ChannelMember>> getChannelMembers(string channelID);
		Task UpsertChannel(Channel channel);
		Task UpsertChannelMember(ChannelMember member);
		Task<List<Following>> getChannelFollowers(string channelID);
		Task<Channel?> getChannel(string channelID);
	}
}