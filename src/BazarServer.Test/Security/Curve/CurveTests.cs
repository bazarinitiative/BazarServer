using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Utils.Security.Curve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils.Security.Curve.Tests
{
	[TestClass()]
	public class CurveTests
	{
		[TestMethod()]
		public void cTest()
		{
			CurveEd25519 ed25519 = new CurveEd25519();
			CurveTestsHelpers.TestCurve(ed25519);

			CurveP521r1 p521R1 = new CurveP521r1 ();
			CurveTestsHelpers.TestCurve(p521R1);

			CurveP256k1 p256k1 = new CurveP256k1();
			CurveTestsHelpers.TestCurve(p256k1);

			CurveEd448 ed448 = new CurveEd448();
			CurveTestsHelpers.TestCurve(ed448);
		}
	}
}