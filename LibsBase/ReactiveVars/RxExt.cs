using System.Reactive.Linq;

namespace ReactiveVars;

public static class RxExt
{
	public static IObservable<Unit> ToUnit<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);
}