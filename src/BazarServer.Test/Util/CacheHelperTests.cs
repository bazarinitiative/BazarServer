using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class CacheHelperTests
	{
		[TestMethod()]
		public void WithCacheTest()
		{
			var start = DateHelper.GetMilliSeconds();

			for (int i = 0; i < 1000; i++)
			{
				int userID = 1234;
				string key = $"userName_{userID}";
				var ret = CacheHelper.WithCache(key, () => GetUserName(userID, i), 5000000);

				ret.Should().Be("Mike0");
			}

			var used = DateHelper.GetMilliSeconds() - start;

			used.Should().BeApproximately(500, 100);


			for (int i = 0; i < 10; i++)
			{
				string key = "GetNullObj";
				var ret = CacheHelper.WithCache(key, () => GetNullObj());
				ret.Should().BeNull();
			}

			for (int i = 0; i < 10; i++)
			{
				string key = "GetIntZero";
				var ret = CacheHelper.WithCache(key, () => GetIntZero());
				ret.Should().Be(0);
			}

			for (int i = 0; i < 10; i++)
			{
				string key = "GetIntOne";
				var ret = CacheHelper.WithCache(key, () => GetIntOne());
				ret.Should().Be(1);
			}

		}

		int GetIntZero()
		{
			return 0;
		}

		int GetIntOne()
		{
			return 1;
		}

		object GetNullObj()
		{
			return null;
		}

		string GetUserName(int userID, int index)
		{
			Thread.Sleep(500);
			return "Mike" + index;
		}

		[TestMethod()]
		public void WithCacheRefreshTest()
		{
		}
	}
}