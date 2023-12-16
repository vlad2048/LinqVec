using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI;

namespace LinqVec.Utils.Rx;

public static class RxExt
{
	public static IObservable<Unit> ToUnitExt<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);
	public static IObservable<T> ObserveOnUI<T>(this IObservable<T> obs) => obs.ObserveOn(Rx.Sched);
}

public static class Rx
{
	public static IScheduler Sched => RxApp.MainThreadScheduler;
}