using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveVars;

public static class RxEventMaker
{
	public static (Action, IObservable<Unit>) Make(Disp d)
	{
		var when = new Subject<Unit>().D(d);
		return (
			() => when.OnNext(Unit.Default),
			when.AsObservable()
		);
	}

	public static (Action<T>, IObservable<T>) Make<T>(Disp d)
	{
		var when = new Subject<T>().D(d);
		return (
			when.OnNext,
			when.AsObservable()
		);
	}
}
