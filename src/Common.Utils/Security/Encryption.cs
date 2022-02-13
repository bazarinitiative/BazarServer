using System.Security.Cryptography;
using System.Text;

namespace Common.Utils
{
	/// <summary>
	/// privateKey should be in pkcs8 format, that's more popular and easier to use.
	/// </summary>
	public static class Encryption
	{
		/// <summary>
		/// generate privateKey and publicKey.
		/// </summary>
		/// <returns></returns>
		public static (string privateKey, string publicKey) GeneratePair()
		{
			//keySize 521 by default
			using ECDsa sa = ECDsa.Create();
			var buf = sa.ExportPkcs8PrivateKey();
			string privateKey = Convert.ToBase64String(buf);
			string publicKey = Convert.ToBase64String(sa.ExportSubjectPublicKeyInfo());

			return (privateKey, publicKey);
		}

		/// <summary>
		/// generate signature.
		/// 
		/// see also https://github.com/dotnet/runtime/issues/30658 
		/// import pkcs8 use SSL functionality in App Service, will requires your app to be in Basic tier or above
		/// 
		/// see also https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code
		/// 
		/// best chioce here is light weight BazarServer do not import any privateKey.
		/// 
		/// ImportECPrivateKey (BER format) seems can work in memory. But pkcs8 (DER format) is more popular and easy to use.
		/// </summary>
		/// <param name="privateKey">pkcs8, base64</param>
		/// <param name="str"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static string Signing(string privateKey, string str, string encoding = "utf-8")
		{
			byte[] bt = Encoding.GetEncoding(encoding).GetBytes(str);

			ECDsa dsa = ECDsa.Create();
			dsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
			var buf = dsa.SignData(bt, HashAlgorithmName.SHA256);
			return Convert.ToBase64String(buf);
		}

		/// <summary>
		/// check signature
		/// </summary>
		/// <param name="strContent">usually a Message body, a json string</param>
		/// <param name="signature"></param>
		/// <param name="publicKey"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static bool CheckSignature(string strContent, string signature, string publicKey, string encoding = "utf-8")
		{
			try
			{
				byte[] bt = Encoding.GetEncoding(encoding).GetBytes(strContent);

				ECDsa dsa = ECDsa.Create();
				dsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
				byte[] rgbSignature = Convert.FromBase64String(signature);
				bool ret = dsa.VerifyData(bt, rgbSignature, HashAlgorithmName.SHA256);
				return ret;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// verify if key pair match
		/// </summary>
		/// <param name="publicKey"></param>
		/// <param name="privateKey"></param>
		/// <returns></returns>
		public static bool VerifyPair(string publicKey, string privateKey)
		{
			string info = "hello, bazar";
			var res = Signing(privateKey, info);
			var check = CheckSignature(info, res, publicKey);
			return check;
		}

		public static string Md5Hash(string plainPassword)
		{
			using var sp = MD5.Create();
			var ret = sp.ComputeHash(plainPassword.GetBytesUtf8());
			var ss = Convert.ToBase64String(ret);
			return ss;
		}

		/// <summary>
		/// calculate the userID by publicKey
		/// </summary>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static string CalculateUserID(string publicKey)
		{
			char[] ay = new char[30];

			for (int i = 0; i < publicKey.Length; i++)
			{
				int pos = i % ay.Length;
				ay[pos] += publicKey[i];
			}

			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			for (int i = 0; i < ay.Length; i++)
			{
				int idx = (int)ay[i] % chars.Length;
				ay[i] = chars[idx];
			}

			return new string(ay);
		}

		static byte[] IV = new byte[16] { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 16 };

		/// <summary>
		/// return base64 of encrypted bytes
		/// </summary>
		/// <param name="plainText">should be 32 bytes long</param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static string AesEncrypt(string plainText, string password)
		{
			using var aesAlg = Aes.Create();
			aesAlg.Key = password.GetBytesUtf8();
			aesAlg.IV = IV;
			aesAlg.Mode = CipherMode.CBC;
			aesAlg.Padding = PaddingMode.PKCS7;

			ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
			var buf = plainText.GetBytesUtf8();
			var encrypted = encryptor.TransformFinalBlock(buf, 0, buf.Length);

			return Convert.ToBase64String(encrypted);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="encrypted">base64</param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static string AesDecrypt(string encrypted, string password)
		{
			using var aesAlg = Aes.Create();
			aesAlg.Key = password.GetBytesUtf8();
			aesAlg.IV = IV;
			aesAlg.Mode = CipherMode.CBC;
			aesAlg.Padding = PaddingMode.PKCS7;

			var buf = Convert.FromBase64String(encrypted);

			ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
			var res = decryptor.TransformFinalBlock(buf, 0, buf.Length);

			return res.GetStringUtf8();
		}
	}
}
