using System.Text;

namespace Common.Utils
{
	public static class DataFormatHelper
	{
		public static string ToHtmlTable(Dictionary<string, string> dic)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<table border='1px' cellpadding='1' cellspacing='1' bgcolor='lightyellow' style='font-family:Garamond; font-size:smaller'>");
			foreach (var key in dic.Keys)
			{
				sb.Append($"<tr><td>{key}</td><td>{dic[key]}</td></tr>");
			}
			sb.Append("</table>");
			return sb.ToString();
		}
	}
}
