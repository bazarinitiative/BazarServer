using BazarServer.Entity.Storage;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class StringExtTests
	{
		[TestMethod()]
		public void GetBytesUtf8Test()
		{
		}

		[TestMethod()]
		public void GetStringUtf8Test()
		{
		}

		[TestMethod()]
		public void LeftTest()
		{
		}

		[TestMethod()]
		public void RightTest()
		{
		}

		[TestMethod()]
		public void ToStringIfNullTest()
		{
		}

		[TestMethod()]
		public void ToHumanBytesTest()
		{
		}

		[TestMethod()]
		public void ToHumanNumberTest()
		{
		}

		[TestMethod()]
		public void IsLetterDigitAllTest()
		{
		}

		[TestMethod()]
		public void IsLetterDigit20Test()
		{
		}

		[TestMethod()]
		public void ToJsonStringTest()
		{
			UserInfo user = null;
			var ss = user.ToJsonString();

			ss.Should().NotBeNull();
		}
	}
}