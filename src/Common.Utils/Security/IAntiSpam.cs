namespace Common.Utils
{
	public interface IAntiSpam
	{
		(bool success, string msg) Check(string type, string spamKey);
	}
}