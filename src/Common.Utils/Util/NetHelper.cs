using System.Net;

namespace Common.Utils.Util
{
	public static class NetHelper
	{
		public static bool IsInternalIP(string testIp)
		{
			if (testIp == "::1") return true;

			byte[] ip = IPAddress.Parse(testIp).GetAddressBytes();
			switch (ip[0])
			{
				case 10:
				case 127:
					return true;
				case 172:
					return ip[1] >= 16 && ip[1] < 32;
				case 192:
					return ip[1] == 168;
				default:
					return false;
			}
		}
	}
}
