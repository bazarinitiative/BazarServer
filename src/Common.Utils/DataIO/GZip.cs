using System.IO.Compression;

namespace Common.Utils
{
	public class GZip
	{
		public static byte[] Compress(byte[] rawData)
		{
			using MemoryStream ms = new MemoryStream();
			using GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
			zip.Write(rawData, 0, rawData.Length);
			zip.Close();
			return ms.ToArray();
		}

		public static byte[] Decompress(byte[] zippedData)
		{
			using MemoryStream ms = new MemoryStream(zippedData);
			using GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
			using MemoryStream outBuffer = new MemoryStream();
			byte[] block = new byte[1024];
			while (true)
			{
				int bytesRead = zip.Read(block, 0, block.Length);
				if (bytesRead <= 0)
					break;
				else
					outBuffer.Write(block, 0, bytesRead);
			}
			zip.Close();
			return outBuffer.ToArray();
		}
	}
}
