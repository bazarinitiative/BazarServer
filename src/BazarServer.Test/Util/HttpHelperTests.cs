using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Text;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class HttpHelperTests
	{
		[TestMethod()]
		public async Task GetAsyncTestAsync()
		{
			var res = await HttpHelper.GetAsync("https://www.yahoo.com");

			res.Length.Should().BePositive();
		}

		[TestMethod()]
		public async Task GetAsyncTest2()
		{
			var ss = await HttpHelper.GetBytesAsync("https://www.yahoo.com/");

			var ddd = Encoding.UTF8.GetString(ss);

			var eee = await HttpHelper.GetAsync("https://www.yahoo.com/");

		}


		[TestMethod()]
		public async Task awaitTest()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			var t1 = GetData();
			var t2 = GetData2();
			var t3 = GetData2();
			var t4 = GetData2();

			var a1 = await t1;
			var a2 = await t2;
			var a3 = await t3;
			var a4 = await t4;

			sw.ElapsedMilliseconds.Should().BeCloseTo(100, 50);
		}

		public async Task<string> GetData()
		{
			await Task.Delay(100);
			return "ok";
		}

		public class AA
		{
			public int aa;
		}

		public async Task<AA> GetData2()
		{
			await Task.Delay(100);
			return new AA();
		}

		[TestMethod()]
		public void PostAsyncTest()
		{
		}

		[TestMethod()]
		public void PostAsyncTest1()
		{
		}

		[TestMethod()]
		public void GetTest()
		{
			var ss = HttpHelper.Get("https://randomuser.me/api/");

			var obj = Json.DeserializeJObject(ss);
			var url = obj["results"][0]["picture"]["thumbnail"].ToString();

			var buf = HttpHelper.GetBytes(url);
		}

	}
}