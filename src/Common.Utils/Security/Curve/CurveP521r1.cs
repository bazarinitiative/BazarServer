using System.Security.Cryptography;
using System.Text;
using static Common.Utils.Security.Curve.Constant;

namespace Common.Utils.Security.Curve
{
	public class CurveP521r1 : ICurve
	{
		public string derivePublicKey(string privateKeyStr)
		{
			ECDsa dsa = ECDsa.Create();
			var buf = BzKeyPair.getKeyBuf(privateKeyStr);
			dsa.ImportPkcs8PrivateKey(buf, out _);
			string publicKey = prefixBzPub + AlgoType.secp521r1 + Base62.ToBase62(dsa.ExportSubjectPublicKeyInfo());
			return publicKey;
		}

		public BzKeyPair generateKeyPair()
		{
			//keySize 521 by default
			using ECDsa sa = ECDsa.Create();
			var priv = sa.ExportPkcs8PrivateKey();
			string privateKey = Base62.ToBase62(priv);
			string publicKey = Base62.ToBase62(sa.ExportSubjectPublicKeyInfo());

			var ret = new BzKeyPair();
			ret.privateKeyStr = prefixBzSec + AlgoType.secp521r1 + privateKey;
			ret.publicKeyStr = prefixBzPub + AlgoType.secp521r1 + publicKey;

			return ret;
		}

		public string sign(string privateKeyStr, string message)
		{
			byte[] bt = Encoding.UTF8.GetBytes(message);

			ECDsa dsa = ECDsa.Create();
			var priv = BzKeyPair.getKeyBuf(privateKeyStr);
			dsa.ImportPkcs8PrivateKey(priv, out _);
			var sig = dsa.SignData(bt, HashAlgorithmName.SHA256);
			return Base62.ToBase62(sig);
		}

		public bool verify(string publicKeyStr, string message, string signature)
		{
			try
			{
				byte[] bt = Encoding.UTF8.GetBytes(message);

				ECDsa dsa = ECDsa.Create();
				var pub = BzKeyPair.getKeyBuf(publicKeyStr);
				dsa.ImportSubjectPublicKeyInfo(pub, out _);
				byte[] sig = Base62.FromBase62(signature);
				bool ret = dsa.VerifyData(bt, sig, HashAlgorithmName.SHA256);
				return ret;
			}
			catch
			{
				return false;
			}
		}
	}
}
