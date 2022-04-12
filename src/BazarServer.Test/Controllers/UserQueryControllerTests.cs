using BazarServer.Application.Query;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BazarServer.Controllers.Tests
{
	[TestClass()]
	public class UserQueryControllerTests : BaseTest
	{
		[TestMethod()]
		public void UserQueryControllerTest()
		{
		}

		[TestMethod()]
		public void GetUserInfoAsyncTest()
		{

		}

		[TestMethod()]
		public void TimelineAsyncTest()
		{
		}

		[TestMethod()]
		public void GetPostsAsyncTest()
		{
		}

		[TestMethod()]
		public void GetUserRetrieveAsyncTest()
		{
		}

		[TestMethod()]
		public void GetProfileAsyncTest()
		{
		}

		[TestMethod()]
		public async Task GetHomeLineTestAsync()
		{
			IUserRepository userRepository = provider.GetService<IUserRepository>();
			IPostRepository postRepository = provider.GetService<IPostRepository>();

			var userID = "k7RmZwQlkX4cbaOl3RGaMoD3LpE2Db";
			var page = 0;
			var pageSize = 20;

			var begin = DateHelper.GetMilliSeconds();
			var users = await userRepository.GetUserFollowees(userID, 0, 1000);
			var ay = users.Select(x => x.targetID).ToList();
			var posts = await PostQueryFacade.GetLatestPostsByUsers(postRepository, ay, page, pageSize);
			var ret = await PostQueryFacade.GetPostDto(postRepository, userID, posts);
			var used = DateHelper.GetMilliSeconds() - begin;
			var ss = used.ToString("0.00");

			Assert.IsTrue(true);
		}

		[TestMethod()]
		public async Task GetNotifiesTestAsync()
		{
			IUserRepository userRepository = provider.GetService<IUserRepository>();
			IPostRepository postRepository = provider.GetService<IPostRepository>();
			var userID = "Kce3xXlO5D8bT4dIeuaq6p6wgAuDr9";
			var startTime = 0;
			var maxCount = 20;

			var ret = await userRepository.GetUserNotify(userID, startTime + 1, maxCount);

			if (ret.Count > 0)
			{
				long maxTime = ret.Max(x => x.notifyTime);
				await userRepository.TryUpdateNotifyGetTime(userID, maxTime);
			}
		}
	}
}