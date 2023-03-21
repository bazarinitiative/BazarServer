using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using static Common.Utils.Security.Curve.Constant;

namespace Common.Utils.Security.Curve
{
	public class CurveEd25519 : ICurve
	{
		public string derivePublicKey(string privateKeyStr)
		{
			var buf = BzKeyPair.getKeyBuf(privateKeyStr);
			Ed25519PrivateKeyParameters param = new Ed25519PrivateKeyParameters(buf);
			var pp = param.GeneratePublicKey();
			var pub = new byte[Ed25519.PublicKeySize];
			pp.Encode(pub);
			string publicKey = Base62.ToBase62(pub);
			var publicKeyStr = prefixBzPub + AlgoType.ed25519 + publicKey;
			return publicKeyStr;
		}

		public BzKeyPair generateKeyPair()
		{
			SecureRandom random = new SecureRandom();
			Ed25519KeyGenerationParameters param = new Ed25519KeyGenerationParameters(random);
			var gen = new Ed25519KeyPairGenerator();
			gen.Init(param);
			var pair = gen.GenerateKeyPair();
			var ecPri = (Ed25519PrivateKeyParameters)pair.Private; 
			var ecPub = (Ed25519PublicKeyParameters)pair.Public;

			var priv = new byte[Ed25519.SecretKeySize];
			var pub = new byte[Ed25519.PublicKeySize];
			ecPri.Encode(priv);
			ecPub.Encode(pub);

			string privateKey = Base62.ToBase62(priv);
			string publicKey = Base62.ToBase62(pub);

			var ret = new BzKeyPair();
			ret.privateKeyStr = prefixBzSec + AlgoType.ed25519 + privateKey;
			ret.publicKeyStr = prefixBzPub + AlgoType.ed25519 + publicKey;

			return ret;
		}

		public string sign(string privateKeyStr, string message)
		{
			var buf = BzKeyPair.getKeyBuf(privateKeyStr);
			Ed25519PrivateKeyParameters param = new Ed25519PrivateKeyParameters(buf);
			Ed25519Signer signer = new Ed25519Signer();
			signer.Init(true, param);
			signer.BlockUpdate(message.GetBytesUtf8());
			var sig = signer.GenerateSignature();
			return Base62.ToBase62(sig);
		}

		public bool verify(string publicKeyStr, string message, string signature)
		{
			var buf = BzKeyPair.getKeyBuf(publicKeyStr);
			Ed25519PublicKeyParameters param = new Ed25519PublicKeyParameters(buf);
			Ed25519Signer signer = new Ed25519Signer();
			signer.Init(false, param);
			signer.BlockUpdate(message.GetBytesUtf8());
			var sig = Base62.FromBase62(signature);
			var ret = signer.VerifySignature(sig);
			return ret;
		}
	}
}
