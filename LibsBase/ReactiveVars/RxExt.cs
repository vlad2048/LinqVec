using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveVars;

public interface IHasDisp : IDisposable
{
	Disp D { get; }
}

public static class RxExt
{
	public static IObservable<T> DisposePrevious<T>(this IObservable<T> source) where T : IDisposable =>
		Obs.Using(
			() => new SerialDisposable(),
			serD => source.Do(e => serD.Disposable = e)
		);


	public static IObservable<Unit> ToUnit<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);

	public static IObservable<T> WhenDisposed<T>(this IRoVar<Option<T>> varOpt, Disp d) where T : IHasDisp =>
		varOpt
			.Select(opt => opt.Match(
				v => v.WhenDisposed(d).Select(_ => v),
				Obs.Never<T>
			))
			.Switch();

	private static IObservable<Unit> WhenDisposed(this IHasDisp hasD, Disp d)
	{
		ISubject<Unit> when = new AsyncSubject<Unit>().D(d);
		Disposable.Create(() =>
		{
			when.OnNext(Unit.Default);
			when.OnCompleted();
		}).D(hasD.D);
		return when.AsObservable();
	}
}