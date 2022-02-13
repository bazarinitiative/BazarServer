using System.Collections.Concurrent;

namespace Common.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class PerfCounter
	{
		double startMilli;
		string logName = "";
		bool writelog = true;

		string instanceID = "PERF" + MyRandom.Random().ToString("0000000000");

		public string InstanceID { get => instanceID; }

		public PerfCounter(string bizName, bool writelog = true)
		{
			startMilli = DateHelper.GetMilliSeconds();
			logName = "perf_" + bizName;
			this.writelog = writelog;

			if (this.writelog)
			{
				LogFacade.LogInformation(logName + " " + instanceID + " " + bizName + " start");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writelog_"></param>
		public void SetWrirelog(bool writelog_)
		{
			writelog = writelog_;
		}

		public void LogOnce(string headmsg)
		{
			if (writelog)
			{
				double elapse = DateHelper.GetMilliSeconds() - startMilli;

				string info = string.Format("{0} - {1} milli", instanceID + " " + headmsg, elapse.ToString("0.00"));
				LogFacade.LogInformation(logName + " " + info);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pointNumber"></param>
		public void LogOnce(int pointNumber)
		{
			LogOnce(pointNumber.ToString());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public double GetElapse()
		{
			double elapse = DateHelper.GetMilliSeconds() - startMilli;
			return elapse;
		}
	}

	public class SlowCounter
	{
		PerfCounter pc;
		int slowMilli;
		ConcurrentDictionary<string, double> cost = new ConcurrentDictionary<string, double>();

		public SlowCounter(string bizName, int slowMilli = 1000)
		{
			pc = new PerfCounter(bizName, false);
			this.slowMilli = slowMilli;
		}

		public void LogOnce(string msg)
		{
			cost.TryAdd(msg, pc.GetElapse());
		}

		public void Finish(string msg)
		{
			cost.TryAdd(msg, pc.GetElapse());

			if (pc.GetElapse() > slowMilli)
			{
				RealLog();
			}
		}

		private void RealLog()
		{
			foreach (var key in cost.Keys)
			{
				var elapse = cost[key];
				string info = string.Format("{0} - {1} milli", pc.InstanceID + " " + key, elapse.ToString("0.00"));
				LogFacade.LogInformation(info);
			}
		}
	}

}