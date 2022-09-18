using Common.Utils;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class MyRandomTests
	{
		[TestMethod()]
		public void RandomTest()
		{
		}

		[TestMethod()]
		public void RandomTest1()
		{
			ConcurrentDictionary<string, int> dic = new ConcurrentDictionary<string, int>();

			Parallel.For(0, 10000, (x) =>
			{
				var key = MyRandom.RandomString(10);
				dic.ContainsKey(key).Should().BeFalse();
				dic.TryAdd(key, 0);
			});
		}

		[TestMethod()]
		public void RandomRateTest()
		{
			Parallel.For(0, 10000, (x) =>
				   {
					   int count = 0;
					   for (int i = 0; i < 5000; i++)
					   {
						   if (MyRandom.RandomRate(10))
						   {
							   count++;
						   }
					   }
					   count.Should().BeCloseTo(500, 100);
				   });
		}

		[TestMethod()]
		public void RandomStringTest()
		{
			var userID = MyRandom.RandomString(20);

			userID.Length.Should().Be(20);
		}

		[TestMethod()]
		public void Random128Test()
		{
			var res = MyRandom.Random128();

			(res >= 0).Should().BeTrue();
		}

		[TestMethod()]
		public void RandomListTest()
		{
			var res = MyRandom.RandomList(0, 100, 100);
			res.Count.Should().Be(100);

			var res2 = MyRandom.RandomList(0, 100, 10);
			res2.Count.Should().Be(10);
		}
	}
}