using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class JsonTests
	{
		class UserA
		{
			public string a { get; set; }
			public string attrib;
			private string pp = "pp";

			public string c { get; set; }
			public int b { get; set; }


			public UserA(string pp_)
			{
				this.pp = pp_;
			}
		}

		class UserB
		{
			public string a { get; set; }
			public string c { get; set; }
			public int b { get; set; }
		}

		[TestMethod()]
		public void SerializeTest()
		{
			var u1 = new UserA("silver");
			u1.attrib = "attrib";

			var s1 = Json.Serialize(u1, true);

			var u2 = Json.Deserialize<UserA>(s1);
			var s2 = Json.Serialize(u2, true);

			Assert.IsTrue(s1 == s2);
		}

		[TestMethod()]
		public void DeserializeTest()
		{
			var u1 = new UserB();
			var s1 = Json.Serialize(u1, true);

			var u2 = Json.Deserialize<UserB>(s1);
			var s2 = Json.Serialize(u2, true);

			Assert.IsTrue(s1 == s2);
		}

		[TestMethod()]
		public void DeserializeTest1()
		{

		}
	}
}