using System.Reactive.Concurrency;
using ReactiveVars;

namespace LogLib.ConTickerLogic.Logic;



class TimeTracker
{
	private readonly IScheduler scheduler;
	private readonly DateTime startTime;
	private readonly IRwVar<TimeRec> timeRec;
	private DateTime Time => scheduler.Now.DateTime;
	private DateTime tickStart;

	public IRoVar<TimeRec> TimeRec => timeRec;
	public bool IsTickTooLong() => Time - tickStart > LogTickerConstants.MaxTickLength;

	public TimeTracker(
		IScheduler scheduler,
		Disp d
	)
	{
		this.scheduler = scheduler;
		timeRec = Var.Make(LogLib.TimeRec.Default, d);
		startTime = Time;
	}

	public void NewTick()
	{
		var now = Time;
		timeRec.V = new TimeRec(now - tickStart, now - startTime);
		tickStart = now;
	}
}