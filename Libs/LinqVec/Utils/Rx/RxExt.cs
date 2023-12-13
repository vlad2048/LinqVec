using System.Reactive.Linq;

namespace LinqVec.Utils.Rx;

public static class RxExt
{
	public static IObservable<Unit> ToUnitExt<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);
}