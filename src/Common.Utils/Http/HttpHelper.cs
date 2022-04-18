using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Common.Utils
{
	public static class HttpHelper
	{
		static HttpClient httpClient;

		static HttpHelper()
		{
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.DefaultConnectionLimit = 100;

			HttpClientHandler handler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = cbServerCertificateValidationCallback,
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				MaxConnectionsPerServer = 100
			};
			httpClient = new HttpClient(handler);
			httpClient.DefaultRequestHeaders.Add("User-Agent", "CommonClient");
			httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
			httpClient.Timeout = TimeSpan.FromSeconds(5);
		}

		private static bool cbServerCertificateValidationCallback(HttpRequestMessage arg1, X509Certificate2? arg2, X509Chain? arg3, SslPolicyErrors arg4)
		{
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static async Task<string> GetAsync(string url)
		{
			using var res = await httpClient.GetAsync(url);
			res.EnsureSuccessStatusCode();
			var responseBody = await res.Content.ReadAsStringAsync();
			return responseBody;
		}

		public static string Get(string url)
		{
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			using var res = httpClient.Send(req);
			res.EnsureSuccessStatusCode();
			using var reader = new StreamReader(res.Content.ReadAsStream());
			return reader.ReadToEnd();
		}

		public static async Task<byte[]> GetBytesAsync(string url)
		{
			using var res = await httpClient.GetAsync(url);
			res.EnsureSuccessStatusCode();
			var responseBody = await res.Content.ReadAsByteArrayAsync();
			return responseBody;
		}

		public static byte[] GetBytes(string url)
		{
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			using var res = httpClient.Send(req);
			res.EnsureSuccessStatusCode();
			using var mem = new MemoryStream();
			res.Content.ReadAsStream().CopyTo(mem);
			return mem.GetBuffer();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="postbody"></param>
		/// <returns></returns>
		public static async Task<string> PostAsync(string url, string postbody, string mediaType = "application/json", string encoding = "utf-8")
		{
			using var content = new StringContent(postbody, Encoding.GetEncoding(encoding), mediaType);
			using var res = await httpClient.PostAsync(new Uri(url), content);
			res.EnsureSuccessStatusCode();
			var responseBody = await res.Content.ReadAsStringAsync();
			return responseBody;
		}

		public static string Post(string url, string postbody, string mediaType = "application/json", string encoding = "utf-8")
		{
			using var content = new StringContent(postbody, Encoding.GetEncoding(encoding), mediaType);
			using var req = new HttpRequestMessage(HttpMethod.Post, url);
			req.Content = content;
			using var res = httpClient.Send(req);
			res.EnsureSuccessStatusCode();
			using var reader = new StreamReader(res.Content.ReadAsStream());
			return reader.ReadToEnd();
		}

		public static string PostForm(string url, IDictionary<string, string> dic)
		{
			var ay = (IEnumerable<KeyValuePair<string?, string?>>)dic;
			using var content = new FormUrlEncodedContent(ay);
			using var req = new HttpRequestMessage(HttpMethod.Post, url);
			req.Content = content;
			using var res = httpClient.Send(req);
			res.EnsureSuccessStatusCode();
			using var reader = new StreamReader(res.Content.ReadAsStream());
			return reader.ReadToEnd();
		}

		public static async Task<string> PostFormAsync(string url, Dictionary<string, string> dic)
		{
			var ay = (IEnumerable<KeyValuePair<string?, string?>>)dic;
			using var content = new FormUrlEncodedContent(ay);
			using var req = new HttpRequestMessage(HttpMethod.Post, url);
			req.Content = content;
			using var res = await httpClient.SendAsync(req);
			res.EnsureSuccessStatusCode();
			using var reader = new StreamReader(res.Content.ReadAsStream());
			return await reader.ReadToEndAsync();
		}

		public static string Post(string url, byte[] postbody)
		{
			using var content = new ByteArrayContent(postbody);
			using var req = new HttpRequestMessage(HttpMethod.Post, url);
			req.Content = content;
			using var res = httpClient.Send(req);
			res.EnsureSuccessStatusCode();
			using var reader = new StreamReader(res.Content.ReadAsStream());
			return reader.ReadToEnd();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="postbody"></param>
		/// <returns></returns>
		public static async Task<string> PostAsync(string url, byte[] postbody)
		{
			using var content = new ByteArrayContent(postbody);
			using var res = await httpClient.PostAsync(new Uri(url), content);
			res.EnsureSuccessStatusCode();
			var responseBody = await res.Content.ReadAsStringAsync();
			return responseBody;
		}

		public static async Task<string> SendAsync(HttpRequestMessage req)
		{
			using var res = await httpClient.SendAsync(req);
			res.EnsureSuccessStatusCode();
			var responseBody = await res.Content.ReadAsStringAsync();
			return responseBody;
		}
	}
}
