using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using PowMaybe;

namespace LinqVec.Tools.Acts;







public delegate IDisposable Act<out Out>(Action start, Action<Out> finish);

public class ActAwaiter<Out> : INotifyCompletion
{
	private readonly Act<Out> act;
	private IObservable<Unit> WhenStart { get; }
	private IObservable<Out> WhenFinish { get; }
	private Maybe<Out> mayResult = May.None<Out>();
	private bool isCompleted;

	public ActAwaiter(Act<Out> act)
	{
		this.act = act;
		(var sigStart, WhenStart) = Sig.Make<Unit>();
		(var sigFinish, WhenFinish) = Sig.Make<Out>();
		IDisposable actD = null!;
		actD = act(
			() => sigStart(Unit.Default),
			v =>
			{
				isCompleted = true;
				sigFinish(v);
				actD.Dispose();
			}
		);
	}

	public bool IsCompleted => isCompleted;

	public void OnCompleted(Action continuation)
	{
		WhenFinish.Subscribe(result =>
		{
			mayResult = May.Some(result);
			continuation();
		});
	}

	public Out GetResult() => mayResult.Ensure();
}

public static class ActExt
{
	public static ActAwaiter<Out> GetAwaiter<Out>(this Act<Out> action)
	{
		return new ActAwaiter<Out>(action);
	}
}


static class Sig
{
	public static (Action<T>, IObservable<T>) Make<T>()
	{
		ISubject<T> when = new AsyncSubject<T>();
		return (
			v =>
			{
				when.OnNext(v);
				when.OnCompleted();
			},
			when.AsObservable()
		);
	}
}