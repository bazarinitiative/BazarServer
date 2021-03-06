using BazarServer.Application.Common;
using BazarServer.Application.Query;
using BazarServer.Infrastructure.Repository;

namespace BazarServer.Controllers;

/// <summary>
/// Send email for verification, backup, etc.
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class EmailController : BazarControllerBase
{
	ILogger<EmailController> logger;
	IConfiguration configuration;
	IAntiSpam antiSpam;
	IUserRepository userRepository;

	string mailAcccount;
	string mailPassword;
	string mailHost;
	int mailPort;
	bool enableSsl;

	public EmailController(ILogger<EmailController> logger, IConfiguration configuration, IAntiSpam antiSpam, IUserRepository userRepository)
	{
		this.logger = logger;
		this.configuration = configuration;

		var mail = ConfigHelper.GetConfigValue(configuration, "BazarMail");

		var ay = mail.Split('_', StringSplitOptions.RemoveEmptyEntries);
		mailAcccount = ay[0];
		mailPassword = ay[1];
		mailHost = ay[2];
		mailPort = Convert.ToInt32(ay[3]);
		enableSsl = bool.Parse(ay[4]);
		this.antiSpam = antiSpam;
		this.userRepository = userRepository;
	}

	static FrequencyControl fcEmailIP = new FrequencyControl(3600, 10);

	/// <summary>
	/// send email with antiSpam, rateLimit
	/// </summary>
	/// <param name="targetEmailAddr"></param>
	/// <param name="title"></param>
	/// <param name="content"></param>
	/// <returns></returns>
	private async Task<ApiResponse> SendInternal(string targetEmailAddr, string title, string content)
	{
		var spam = antiSpam.Check("Client.Command", HttpContext.GetRealIP());
		if (!spam.success)
		{
			return Error(spam.msg);
		}

		var ip = HttpContext.GetRealIP();
		if (!fcEmailIP.Check(ip))
		{
			return Error($"email too much for IP: {ip}");
		}

		try
		{
			MailHelper.mailHost = mailHost;
			MailHelper.mailPort = mailPort;
			MailHelper.mailAcccount = mailAcccount;
			MailHelper.mailPassword = mailPassword;

			logger.LogInformation($"SendMail from {mailAcccount} to {targetEmailAddr}, ssl={enableSsl}");
			await MailHelper.SendMail(mailAcccount, targetEmailAddr, title, content, true, null, enableSsl, "Bazar");
		}
		catch (Exception ex)
		{
			return Error(ex.Message);
		}

		return Success();
	}

	public class SendCodeReq
	{
		public string targetEmailAddr { get; set; } = "";
		public string code { get; set; } = "";
		public string lang { get; set; } = "en";
	}

	/// <summary>
	/// send a verification code through email
	/// </summary>
	/// <returns></returns>
	[HttpPost]
	public async Task<ApiResponse> SendCode(SendCodeReq req)
	{
		string title = $"Verification Code";
		string content = @$"

Your verification code is <div style='color: red'><big>{req.code}</big></div>

";

		return await SendInternal(req.targetEmailAddr, title, content);
	}

	public class BackupAccountReq
	{
		public string targetEmailAddr { get; set; } = "";
		public string publicKey { get; set; } = "";
		public string privateKey { get; set; } = "";
		public string lang { get; set; } = "en";
	}

	/// <summary>
	/// backup user account data through email.
	/// 
	/// see also https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code
	/// </summary>
	/// <returns></returns>
	[HttpPost]
	public async Task<ApiResponse> BackupAccount(BackupAccountReq req)
	{
		try
		{
			string title = $"Backup Account";
			var targetEmailAddr = req.targetEmailAddr.Trim();
			var publicKey = req.publicKey.Trim();
			var privateKey = req.privateKey.Trim();

			//see also https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code
			//best chioce here is light weight BazarServer do not import any privateKey
			//if (!Encryption.VerifyPair(publicKey, privateKey))
			//{
			//	return new ApiResponse(false, "Key pair not match");
			//}

			var userID = Encryption.CalculateUserID(publicKey);
			var userDto = await UserQueryFacade.GetUserDto_WithCache(userRepository, userID);
			var userName = userDto.userInfo.userName ?? "";

			Dictionary<string, string> dic = new Dictionary<string, string>();
			dic.Add("Public Key", "Private Key");
			dic.Add(publicKey, privateKey);
			string content = @$"
<html><body><div>
UserID: <span style='color: grey'>{userID}</span><br>
UserName: <span style='color: grey'>{userName}</span><br>
Your backup account data is <br>
<div style='max-width: 400px; word-break: break-all;'>
{DataFormatHelper.ToHtmlTable(dic)}
</div>
</div></body></html>
";
			return await SendInternal(targetEmailAddr, title, content);
		}
		catch (Exception ex)
		{
			return Error(ex.ToString());
		}
	}

}
