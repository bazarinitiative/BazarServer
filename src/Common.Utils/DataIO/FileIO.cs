using System.Text;

namespace Common.Utils
{
	public static class FileIO
	{

		public static void EnsureDirectory(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}

		/// <summary>
		/// read file as UTF8 string
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		public static string ReadAllText(string filepath)
		{
			var ss = File.ReadAllText(filepath, Encoding.UTF8);
			return ss;
		}

		public static async Task<string> ReadAllTextAsync(string filepath)
		{
			var ss = await File.ReadAllTextAsync(filepath, Encoding.UTF8);
			return ss;
		}

		public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
		{
			using MemoryStream ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			var ret = ms.ToArray();
			return ret;
		}

		public static byte[] ReadAllBytes(this Stream stream)
		{
			using MemoryStream ms = new MemoryStream();
			stream.CopyTo(ms);
			var ret = ms.ToArray();
			return ret;
		}
	}
}
