using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveVars;

namespace LinqVec.Utils.Rx;

public static class RxExt
{
	public static IObservable<Unit> ToUnit<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);
	public static IObservable<T> ObserveOnUI<T>(this IObservable<T> obs) => obs.ObserveOn(Rx.Sched);
}

public static class Rx
{
	public static IScheduler Sched => RxApp.MainThreadScheduler;

	public static Disp MkUID(Disp topD)
	{
		var d = MkD();
		new ScheduledDisposable(Sched, d).D(topD);
		return d;
	}
}