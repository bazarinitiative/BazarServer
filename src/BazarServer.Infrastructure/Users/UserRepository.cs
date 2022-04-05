using BazarServer.Entity.Storage;
using Common.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BazarServer.Infrastructure.Users
{
	public class UserRepository : IUserRepository
	{
		private readonly IGenericMongoCollection<UserInfo> _conn;
		private readonly IGenericMongoCollection<UserPic> _connPic;
		private readonly IGenericMongoCollection<Following> _followings;
		private readonly IGenericMongoCollection<Like> _connLike;
		private readonly IGenericMongoCollection<UserStatistic> _connStat;
		private readonly IGenericMongoCollection<NotifyMessage> _connNoti;
		private readonly IGenericMongoCollection<NotifyGetTime> _connNotiGetTime;
		private readonly ILogger<UserRepository> _logger;

		public UserRepository(IGenericMongoCollection<UserInfo> conn, IGenericMongoCollection<UserStatistic> connStat,
						ILogger<UserRepository> logger, IGenericMongoCollection<UserPic> connPic,
						IGenericMongoCollection<Following> followings, IGenericMongoCollection<NotifyMessage> connNoti,
						IGenericMongoCollection<NotifyGetTime> connNotiGetTime, IGenericMongoCollection<Like> connLike)
		{
			_conn = conn;
			_connStat = connStat;
			_logger = logger;
			_connPic = connPic;
			_followings = followings;
			_connNoti = connNoti;
			_connNotiGetTime = connNotiGetTime;
			_connLike = connLike;
		}

		public async Task SaveUserAsync(UserInfo model)
		{
			await _conn.UpsertAsync(x => x.userID == model.userID, model);
		}

		public async Task<UserInfo?> GetUserInfoAsync(string userID)
		{
			var model = await _conn.FirstOrDefaultAsync(x => x.userID == userID);
			return model;
		}

		public async Task<UserInfo?> GetUserInfoAsync_WithCache(string userID, int cacheMilli)
		{
			var ret = await CacheHelper.WithCacheAsync(
											"userinfo_" + userID,
											async () => await GetUserInfoAsync(userID),
											cacheMilli,
											false
											);
			return ret;
		}


		public async Task<UserStatistic?> GetUserStatisticAsync(string userID)
		{
			var ret = await _connStat.FirstOrDefaultAsync(x => x.userID == userID);
			return ret;
		}

		public async Task UpsertUserStatisticAsync(string userID, int addPostCount = 0, int addLikedCount = 0, int addFollowingCount = 0, int addFollowedCount = 0)
		{
			try
			{
				var ret = await _connStat.FirstOrDefaultAsync(x => x.userID == userID);
				if (ret == null)
				{
					ret = new UserStatistic(userID);
				}
				ret.postCount += addPostCount;
				ret.likedCount += addLikedCount;
				ret.followingCount += addFollowingCount;
				ret.followedCount += addFollowedCount;
				await _connStat.UpsertAsync(x => x.userID == userID, ret);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"user:{userID}");
			}
		}

		public async Task<UserPic?> GetUserPicAsync(string userID)
		{
			var ret = await _connPic.FirstOrDefaultAsync(x => x.userID == userID);
			return ret;
		}

		public async Task<bool> IsExistUserAsync(string userID)
		{
			var user = await GetUserInfoAsync(userID);
			return (user != null);
		}

		public async Task<List<Following>> GetUserFollowers(string userID, int page, int pageSize)
		{
			var ret = await _followings.PageAsync(x => x.targetID == userID && x.targetType == "User",
											x => x.commandTime, page, pageSize, true);
			return ret;
		}

		public async Task<List<Following>> GetUserFollowees(string userID, int page, int pageSize)
		{
			var ret = await _followings.PageAsync(x => x.userID == userID && x.targetType == "User",
											x => x.commandTime, page, pageSize, true);
			return ret;
		}

		public async Task<Following?> GetUserFollowing(string userID, string targetID)
		{
			var ay = await _followings.GetAsync(x => x.userID == userID && x.targetID == targetID);
			var ret = ay.FirstOrDefault();
			return ret;
		}

		public async Task AddUserNotify(NotifyMessage noti)
		{
			await _connNoti.UpsertAsync(x => x.notifyID == noti.notifyID, noti);
		}

		public async Task RemoveNotify(string fromWhere)
		{
			await _connNoti.RemoveAsync(x => x.fromWhere == fromWhere);
		}

		public async Task<List<NotifyMessage>> GetUserNotify(string userID, long startTime, int backwardCount = 1000)
		{
			if (startTime == 0)
			{
				startTime = DateHelper.CurrentTimeMillis();
			}
			var ret = await _connNoti.PageAsync(x => x.userID == userID && x.notifyTime <= startTime, x => x.notifyTime, 0, backwardCount, true);
			return ret;
		}

		public async Task TryUpdateNotifyGetTime(string userID, long getTime)
		{
			var rec = await _connNotiGetTime.FirstOrDefaultAsync(x => x.userID == userID);
			if (rec == null || rec.getTime < getTime)
			{
				var newrec = new NotifyGetTime()
				{
					userID = userID,
					getTime = getTime,
				};
				await _connNotiGetTime.UpsertAsync(x => x.userID == userID, newrec);
			}
		}

		public async Task<long> GetNewNotifyCount(string userID)
		{
			long getTime = 0;
			var rec = await _connNotiGetTime.FirstOrDefaultAsync(x => x.userID == userID);
			if (rec != null)
			{
				getTime = rec.getTime;
			}
			var ret = await _connNoti.CountAsync(x => x.notifyTime > getTime);
			return ret;
		}

		public async Task<List<UserInfo>> GetRandomUser(int count)
		{
			var ret = await _conn.Random(count);
			return ret;
		}

		public async Task<List<UserInfo>> Search(List<string> ay, int startIdx, int endIdx)
		{
			var ret = await _conn.Search(ay, startIdx, endIdx, x => x.createTime);
			return ret;
		}

		public async Task<List<Like>> GetUserLikes(string userID, int page, int pageSize)
		{
			var ret = await _connLike.PageAsync(x => x.userID == userID, x => x.commandTime, page, pageSize, true);
			return ret;
		}
	}
}
