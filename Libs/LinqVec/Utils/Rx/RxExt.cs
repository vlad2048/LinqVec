using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData.Kernel;
using ReactiveUI;
using ReactiveVars;

namespace LinqVec.Utils.Rx;

public static class Rx
{
	public static IScheduler Sched => RxApp.MainThreadScheduler;
}

public static class RxExt
{
	//public static IObservable<string> TimePrefix<T>(this IObservable<T> source, IScheduler scheduler) =>
	//	source
	//		.Timestamp(scheduler)
	//		.Select(e => $"[{e.Timestamp:HH:mm:ss.fffffff}] - {e.Value}");

	public static IObservable<T> ObserveOnUI<T>(this IObservable<T> obs) => obs.ObserveOn(Rx.Sched);
	public static IObservable<Option<U>> Map2<T, U>(this IObservable<Option<T>> obs, Func<T, U> fun) => obs.Select(e => e.Map(fun));

	public static (IObservable<T>, Action<bool>) TerminateWithAction<T>(this IObservable<T> source)
	{
		var subj = new AsyncSubject<bool>();
		void Finish(bool commit)
		{
			subj.OnNext(commit);
			subj.OnCompleted();
			subj.Dispose();
		}

		var destination =
			Obs.Using(
				() => MkD($"TerminateWithAction<{typeof(T).Name}>"),
				d => Obs.Create<T>(obs =>
				{
					source.Subscribe(obs.OnNext).D(d);
					subj.Subscribe(commit =>
					{
						if (commit)
							obs.OnCompleted();
						else
							obs.OnError(new ArgumentException("User cancelled"));
					}).D(d);
					return d;
				})
			);
		return (destination, Finish);
	}
}
