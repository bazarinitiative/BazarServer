using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Utils.Security.Curve.Tests
{
	internal static class CurveTestsHelpers
	{

		public static void TestCurve(ICurve k1)
		{
			var pair = k1.generateKeyPair();
			var newpub = k1.derivePublicKey(pair.privateKeyStr);
			Assert.IsTrue(pair.publicKeyStr == newpub);

			var messsage = "aaabbbccddefg";
			var sig = k1.sign(pair.privateKeyStr, messsage);
			var v1 = k1.verify(pair.publicKeyStr, messsage, sig);
			var v2 = k1.verify(pair.publicKeyStr, messsage + "123", sig);
			Assert.IsTrue(v1);
			Assert.IsFalse(v2);
		}
	}
}