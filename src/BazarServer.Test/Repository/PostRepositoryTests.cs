using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Common.Utils;

namespace BazarServer.Infrastructure.Repository.Tests
{
	[TestClass()]
	public class PostRepositoryTests : BaseTest
	{
		[TestMethod()]
		public async Task GetPostsByUserAsyncTest()
		{
			IPostRepository postRepository = provider.GetService<IPostRepository>();

			var userID = "CZYZBUpXOtxtxmsHv7FexLf34bGdRX";
			var begin = DateHelper.GetMilliSeconds();
			var ret = await postRepository.GetPostsByUserAsync(userID, false, 0, 100);
			var used = DateHelper.GetMilliSeconds() - begin;
			var ss = used.ToString("0.00");
		}
	}
}