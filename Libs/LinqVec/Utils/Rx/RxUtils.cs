using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace LinqVec.Utils.Rx;

static class RxUtils
{
	public static IObservable<T> IfOtherDoesntHappenWithin<T, U>(this IObservable<T> src, IObservable<U> other, TimeSpan delay, IScheduler scheduler) =>
		src
			.Select(e =>
				Obs.Amb(
						Obs.Return((Allowed: true, Value: e)).Delay(delay, scheduler),
						other.Select(_ => (Allowed: false, Value: e))
					)
					.Take(1)
					.Where(t => t.Allowed)
					.Select(t => t.Value)
			)
			.Switch();

	public static IObservable<T> SpotSequenceReturnFirst<T, U>(this IObservable<T> src, U[] seq, Func<U, T, bool> matchFun) =>
		src
			.Buffer(seq.Length, 1)
			.Where(e => e.AreSeqEqual(seq, matchFun))
			.Select(e => e.First());

	private static bool AreSeqEqual<T, U>(this ICollection<T> xs, ICollection<U> ys, Func<U, T, bool> matchFun) => xs.Count == ys.Count && xs.Zip(ys).All(t => matchFun(t.Item2, t.Item1));
}