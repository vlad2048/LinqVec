using System;
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
	public static IObservable<T> DupWhen<T>(this IObservable<T> source, IObservable<Unit> whenDup) =>
		Obs.Merge(
			whenDup.WithLatestFrom(source, (_, v) => v),
			source
		);


	public static IRoVar<T> InvokeAndSequentiallyDispose<T>(this IObservable<Func<Disp, T>> sourceFun) =>
		Obs.Using(
				() => new SerialDisposable(),
				serD => sourceFun
					.Select(fun =>
					{
						serD.Disposable = null;
						var stateD = MkD($"InvokeAndSequentiallyDispose<{typeof(T).Name}>");
						var state = fun(stateD);
						serD.Disposable = stateD;
						return state;
					})
			)
			.ToVar();


	public static IObservable<U> SwitchOption_NeverIfNone<T, U>(this IObservable<Option<T>> source, Func<T, IObservable<U>> fun) =>
		source
			.Select(e => e.Match(
				fun,
				Obs.Never<U>
			))
			.Switch();



	/*public static void DisposePreviousSequentiallyOrWhen(
		this IObservable<Func<IDisposable>> source,
		IObservable<Unit> whenDispose,
		DISP d
	) =>
		Obs.Using(
				() => new SequentialSerialDisposable(),
				serD =>
					Obs.Merge(
						source.Do(e => serD.DisposableFun = e).ToUnit(),
						whenDispose.Do(_ => serD.DisposableFun = null)
					)
			)
			.MakeHot(d);*/


	public static IObservable<Unit> ToUnit<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);






	public static IObservable<Unit> WhenDisposed<T>(this IObservable<Option<T>> source) where T : IHasDisp =>
		source
			.Select(e => e.Match(
				f => f.WhenDisposed(),
				Obs.Never<Unit>
			))
			.Switch();
	public static IObservable<Unit> WhenDisposed<T>(this IObservable<T> source) where T : IHasDisp => source.Select(e => e.WhenDisposed()).Switch();
	public static IObservable<Unit> WhenDisposed<T>(this T hasD) where T : IHasDisp => hasD.D.WhenDisposed();
	public static IObservable<Unit> WhenDisposed(this Disp d) =>
		Obs.Using(
			() => new Disp(),
			obsD =>
			{
				ISubject<Unit> when = new AsyncSubject<Unit>().D(obsD);
				Disposable.Create(() =>
				{
					when.OnNext(Unit.Default);
					when.OnCompleted();
				}).D(d).D(obsD);
				return when.AsObservable();
			}
		);
}