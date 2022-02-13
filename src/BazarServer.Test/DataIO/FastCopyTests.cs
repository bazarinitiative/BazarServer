using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class FastCopyTests
	{
		class AA
		{
			public int p1 = 1;
			public int p2 { get; set; } = 1;

			private int p3 = 1;

			public int GetP3()
			{
				return p3;
			}
		}

		class BB
		{
			public int p1 = 2;
			public int p2 { get; set; } = 2;

			private int p3 = 2;

			public int GetP3()
			{
				return p3;
			}
		}

		[TestMethod()]
		public void CopyTest()
		{
			AA a = new AA();
			BB b = new BB();
			FastCopy.Copy(a, b);

			Assert.IsTrue(b.p1 == 1 && b.p2 == 1 && b.GetP3() == 2);
		}

		[TestMethod()]
		public void CopyPropertiesTest()
		{
		}

		[TestMethod()]
		public void CopyFieldsTest()
		{
		}
	}
}