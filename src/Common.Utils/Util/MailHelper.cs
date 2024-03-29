﻿using System.Net.Mail;
using System.Text;

namespace Common.Utils
{
	public static class MailHelper
	{
		public static string? mailHost { get; set; }
		public static int mailPort { get; set; }
		public static string? mailAcccount { get; set; }
		public static string? mailPassword { get; set; }

		/// <summary>
		/// beware, many smtp server do not support ssl and should use port 25
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="title"></param>
		/// <param name="body"></param>
		/// <param name="isBodyHtml"></param>
		/// <param name="bodyEncoding">null will default to UTF8</param>
		/// <param name="enableSsl"></param>
		/// <returns></returns>
		public static async Task SendMail(string from, string to, string title, string body, bool isBodyHtml = true, Encoding? bodyEncoding = null, bool enableSsl = false, string displayFromName = "")
		{
			using MailMessage message = new MailMessage(from, to, title, body);
			message.From = new MailAddress(from, displayFromName);
			message.IsBodyHtml = isBodyHtml;
			if (bodyEncoding == null)
			{
				bodyEncoding = Encoding.UTF8;
			}
			message.BodyEncoding = bodyEncoding;
			using SmtpClient client = new SmtpClient(mailHost, mailPort);
			client.UseDefaultCredentials = false;
			client.Credentials = new System.Net.NetworkCredential(mailAcccount, mailPassword);
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.EnableSsl = enableSsl;
			client.Timeout = 5000;
			await client.SendMailAsync(message);
		}

	}
}
