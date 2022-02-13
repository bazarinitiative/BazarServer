using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Utils.Tests
{
	[TestClass()]
	public class EncryptionTests
	{
		[TestMethod()]
		public void GeneratePairTest()
		{
			var pub = "MIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQBcMUIw2cgunzGmOtMg1P/0dI0RaDG62Jh+alFqNY1l0idZc84mFXalvsvEdsir0FlXdvjxdglOc8r8blQPi5+U7ABosVTUJFts7/bHQAKv3G4P7eNMsR8w4fnSG9vkW378AAIh0qOVUfh3k8qFF0d18RpeoTrXHidFgxiwV2iytZm5+0=";
			var priv = "MIHuAgEAMBAGByqGSM49AgEGBSuBBAAjBIHWMIHTAgEBBEIBV7g+CmRpHGOvXBEw0Y6RdZs4y2hdWc1aW21bNm37HUWu3bKj1pwJkTXvWkQkHCKWYXsR+jfs0+D6GQE20RhYHR2hgYkDgYYABAFwxQjDZyC6fMaY60yDU//R0jRFoMbrYmH5qUWo1jWXSJ1lzziYVdqW+y8R2yKvQWVd2+PF2CU5zyvxuVA+Ln5TsAGixVNQkW2zv9sdAAq/cbg/t40yxHzDh+dIb2+RbfvwAAiHSo5VR+HeTyoUXR3XxGl6hOtceJ0WDGLBXaLK1mbn7Q==";
			Encryption.VerifyPair(pub, priv).Should().BeTrue();

			var res = Encryption.GeneratePair();
			_ = res.publicKey.Length;
			_ = res.privateKey.Length;

			string content = "hello, world!";
			var signature = Encryption.Signing(res.privateKey, content);

			Encryption.CheckSignature(content, signature, res.publicKey).Should().BeTrue();
			Encryption.CheckSignature(content + "a", signature, res.publicKey).Should().BeFalse();

			Encryption.VerifyPair(res.publicKey, res.privateKey).Should().BeTrue();
		}


		[TestMethod()]
		public void SigningTest()
		{
			var pair = Encryption.GeneratePair();

			var content = "hello";

			var sig = Encryption.Signing(pair.privateKey, content);

			var ret = Encryption.CheckSignature(content, sig, pair.publicKey);

			ret.Should().BeTrue();
		}

		[TestMethod()]
		public void CheckSignatureTest()
		{
		}


		[TestMethod()]
		public void CalculateUserIDTest()
		{
			var pair = Encryption.GeneratePair();
			var userID = Encryption.CalculateUserID(pair.publicKey);

			userID.Length.Should().Be(30);
		}

	}
}
