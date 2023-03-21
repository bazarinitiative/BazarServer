using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using static Common.Utils.Security.Curve.Constant;

namespace Common.Utils.Security.Curve
{
	public class CurveP256k1 : ICurve
	{
		static X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
		static ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

		public string derivePublicKey(string privateKeyStr)
		{
			var buf = BzKeyPair.getKeyBuf(privateKeyStr);

			BigInteger d = new BigInteger(buf);
			ECPoint q = domain.G.Multiply(d);
			var publicParams = new ECPublicKeyParameters(q, domain);
			var pub = publicParams.Q.GetEncoded();
			var ret = prefixBzPub + AlgoType.secp256k1 + Base62.ToBase62(pub);
			return ret;
		}

		public BzKeyPair generateKeyPair()
		{
			var secureRandom = new SecureRandom();
			var keyParams = new ECKeyGenerationParameters(domain, secureRandom);
			var generator = new ECKeyPairGenerator("ECDSA");
			generator.Init(keyParams);
			var pair = generator.GenerateKeyPair();
			ECPublicKeyParameters ecPub = (ECPublicKeyParameters)pair.Public;
			ECPrivateKeyParameters ecPri = (ECPrivateKeyParameters)pair.Private;

			var pub = ecPub.Q.GetEncoded();
			var priv = ecPri.D.ToByteArrayUnsigned();

			string privateKey = Base62.ToBase62(priv);
			string publicKey = Base62.ToBase62(pub);

			var ret = new BzKeyPair();
			ret.privateKeyStr = prefixBzSec + AlgoType.secp256k1 + privateKey;
			ret.publicKeyStr = prefixBzPub + AlgoType.secp256k1 + publicKey;

			return ret;
		}

		public string sign(string privateKeyStr, string message)
		{
			var buf = BzKeyPair.getKeyBuf(privateKeyStr);
			BigInteger d = new BigInteger(buf);

			ECPrivateKeyParameters param = new ECPrivateKeyParameters(d, domain);

			var signer = SignerUtilities.GetSigner("ECDSAWITHSHA-256");
			signer.Init(true, param);
			signer.BlockUpdate(message.GetBytesUtf8());
			var sig = signer.GenerateSignature();
			var ret = Base62.ToBase62(sig);

			return ret;
		}

		public bool verify(string publicKeyStr, string message, string signature)
		{
			var buf = BzKeyPair.getKeyBuf(publicKeyStr);
			var q = curve.Curve.DecodePoint(buf);

			ECPublicKeyParameters key = new ECPublicKeyParameters(q, domain);
			var signer = SignerUtilities.GetSigner("ECDSAWITHSHA-256");
			signer.Init(false, key);
			signer.BlockUpdate(message.GetBytesUtf8());
			return signer.VerifySignature(Base62.FromBase62(signature));
		}
	}
}
