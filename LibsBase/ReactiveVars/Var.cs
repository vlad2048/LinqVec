using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PowRxVar;

public static class Var
{
	public static IRwVar<T> Make<T>(this T init) => new RwVar<T>(init);

	public static IRoVar<T> ToVar<T>(this IObservable<T> obs) => new RoVar<T>(obs);

	public static IRoVar<T> ToVar<T>(this IObservable<T> obs, Disp d) => obs.MakeReplay(d).ToVar();

	public static IObservable<T> MakeReplay<T>(this IObservable<T> src, Disp d)
	{
		var srcConn = src.Replay(1);
		srcConn.Connect().D(d);
		return srcConn;
	}

	public static IObservable<T> MakeHot<T>(this IObservable<T> src, Disp d)
	{
		var srcConn = src.Publish();
		srcConn.Connect().D(d);
		return srcConn;
	}


	private sealed class RwVar<T> : IRwVar<T>
	{
		public void Dispose() => Subj.Dispose();
		public IDisposable Subscribe(IObserver<T> observer) => Subj.Subscribe(observer);

		private readonly BehaviorSubject<T> Subj;

		public T V
		{
			get => Subj.Value;
			set => Subj.OnNext(value);
		}
		public bool IsDisposed => Subj.IsDisposed;

		public RwVar(T init)
		{
			Subj = new BehaviorSubject<T>(init);
		}
	}

	private sealed class RoVar<T> : IRoVar<T>
	{
		private readonly IObservable<T> obs;

		public IDisposable Subscribe(IObserver<T> observer) => obs.Subscribe(observer);
		public T V => Task.Run(async () => await obs.FirstAsync()).Result;

		public RoVar(IObservable<T> obs)
		{
			this.obs = obs;
		}
	}
}