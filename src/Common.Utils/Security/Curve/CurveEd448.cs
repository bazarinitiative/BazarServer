using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Utils.Security.Curve.Constant;

namespace Common.Utils.Security.Curve
{
	public class CurveEd448 : ICurve
	{
		public string derivePublicKey(string privateKeyStr)
		{
			var buf = BzKeyPair.getKeyBuf(privateKeyStr);
			Ed448PrivateKeyParameters param = new Ed448PrivateKeyParameters(buf);
			var pp = param.GeneratePublicKey();
			var pub = new byte[Ed448.PublicKeySize];
			pp.Encode(pub);
			string publicKey = Base62.ToBase62(pub);
			var publicKeyStr = prefixBzPub + AlgoType.ed448 + publicKey;
			return publicKeyStr;
		}

		public BzKeyPair generateKeyPair()
		{
			SecureRandom random = new SecureRandom();
			Ed448KeyGenerationParameters param = new Ed448KeyGenerationParameters(random);
			var gen = new Ed448KeyPairGenerator();
			gen.Init(param);
			var pair = gen.GenerateKeyPair();
			var ecPri = (Ed448PrivateKeyParameters)pair.Private;
			var ecPub = (Ed448PublicKeyParameters)pair.Public;

			var priv = new byte[Ed448.SecretKeySize];
			var pub = new byte[Ed448.PublicKeySize];
			ecPri.Encode(priv);
			ecPub.Encode(pub);

			string privateKey = Base62.ToBase62(priv);
			string publicKey = Base62.ToBase62(pub);

			var ret = new BzKeyPair();
			ret.privateKeyStr = prefixBzSec + AlgoType.ed448 + privateKey;
			ret.publicKeyStr = prefixBzPub + AlgoType.ed448 + publicKey;

			return ret;
		}

		static byte[] EMPTY_CONTEXT = new byte[0];

		public string sign(string privateKeyStr, string message)
		{
			var buf = BzKeyPair.getKeyBuf(privateKeyStr);
			Ed448PrivateKeyParameters param = new Ed448PrivateKeyParameters(buf);
			Ed448Signer signer = new Ed448Signer(EMPTY_CONTEXT);
			signer.Init(true, param);
			signer.BlockUpdate(message.GetBytesUtf8());
			var sig = signer.GenerateSignature();
			return Base62.ToBase62(sig);
		}

		public bool verify(string publicKeyStr, string message, string signature)
		{
			var buf = BzKeyPair.getKeyBuf(publicKeyStr);
			Ed448PublicKeyParameters param = new Ed448PublicKeyParameters(buf);

			Ed448Signer signer = new Ed448Signer(EMPTY_CONTEXT);
			signer.Init(false, param);
			signer.BlockUpdate(message.GetBytesUtf8());
			var sig = Base62.FromBase62(signature);
			var ret = signer.VerifySignature(sig);
			return ret;
		}
	}
}
