using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class ConfigHelperTests
	{
		[TestMethod()]
		public void GetConfigValueTest()
		{
			var ss = ConfigHelper.GetConfigValue(null, "BazarMail");
		}
	}
}