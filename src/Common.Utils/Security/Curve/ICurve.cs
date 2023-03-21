using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils.Security.Curve
{
	public static class Constant
	{
		public const string prefixBzPub = "bpub";
		public const string prefixBzSec = "bsec";

		public class AlgoType
		{
			public const char secp521r1 = '1';      //aka nistp521
			public const char secp256k1 = '2';
			public const char ed25519 = '3';
			public const char ed448 = '4';
			public const char secpmax = '9';
		}
	}

	public class BzKeyPair
	{
		/**
		 * prefixBzPub + AlgoType + base62_publicKey
		 */
		public string publicKeyStr = "";
		/**
		 * prefixBzPriv + AlgoType + base62_privateKey
		 * As forward compatbility, those old privateKeyStr start with 'MIH' will be treated as secp521r1 privateKey without prefix.
		 */
		public string privateKeyStr = "";

		/**
		 * extract base62 part of keyStr and convert to buf
		 * @param keyStr 
		 * @returns 
		 */
		static public byte[] getKeyBuf(string keyStr)
		{
			if (keyStr.StartsWith(Constant.prefixBzSec))
			{
				var sub = keyStr.Substring(Constant.prefixBzSec.Length + 1);
				return Base62.FromBase62(sub);
			}
			else if (keyStr.StartsWith(Constant.prefixBzPub))
			{
				var sub = keyStr.Substring(Constant.prefixBzPub.Length + 1);
				return Base62.FromBase62(sub);
			}
			else
			{
				throw new Exception("unsupported keyStr: " + keyStr);
			}
		}
	}

	public interface ICurve
	{
		BzKeyPair generateKeyPair();

		/**
		 * will return BzKeyPair.publicKeyStr
		 * @param privateKeyStr should be BzKeyPair.privateKeyStr
		 */
		string derivePublicKey(string privateKeyStr);

		/**
		 * signature in base62
		 * @param privateKeyStr private key
		 * @param message content msg to sign
		 */
		string sign(string privateKeyStr, string message);

		/**
		 * 
		 * @param publicKeyStr 
		 * @param message content msg to verify
		 * @param signature signature to verify
		 */
		bool verify(string publicKeyStr, string message, string signature);
	}
}
