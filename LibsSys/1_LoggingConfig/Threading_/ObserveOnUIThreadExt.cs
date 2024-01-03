using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace LoggingConfig.Threading_;

public static class ObserveOnUIThreadExt
{
	private static SynchronizationContextScheduler? scheduler;
	private static SynchronizationContextScheduler Scheduler => scheduler ?? throw new NullReferenceException();

	public static IObservable<T> ObserveOnUIThread<T>(this IObservable<T> obs)
	{
		InitIFN();
		return obs.ObserveOn(SynchronizationContext.Current ?? throw new ArgumentException());
	}

	private static void InitIFN()
	{
		if (scheduler != null) return;
		var syncContext = new WindowsFormsSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(syncContext);
		scheduler = new SynchronizationContextScheduler(syncContext);
	}
}