using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveVars;

public static class Var
{
	public static IRwVar<T> Make<T>(this T init, Disp d) => new RwVar<T>(init, d);
	public static IRoVar<T> MakeConst<T>(T val) => Obs.Return(val).ToVar();
	public static IBoundVar<T> MakeBound<T>(T init, Disp d) => new BoundVar<T>(init, d);
	public static IRoVar<Option<T>> MakeOptionalFromOptionalObs<T>(IObservable<Option<T>> source, Disp d) => source.Prepend(None).ToVar(d);

	public static IRoVar<T> ToVar<T>(this IObservable<T> obs) => new RoVar<T>(obs.Replay(1).RefCount());
	public static IRoVar<T> ToVar<T>(this IObservable<T> obs, Disp d) => new RoVar<T>(obs.MakeReplay(d));


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


	private sealed class RwVar<T> : IRwVar<T>
	{
		public Disp D { get; }
		public void Dispose() => D.Dispose();
		public IDisposable Subscribe(IObserver<T> observer) => Subj.Subscribe(observer);

		private readonly BehaviorSubject<T> Subj;

		public T V
		{
			get => Subj.Value;
			set => Subj.OnNext(value);
		}

		public bool IsDisposed => Subj.IsDisposed;

		public RwVar(T init, Disp d)
		{
			D = d;
			Subj = new BehaviorSubject<T>(init).D(d);
		}
	}


	private sealed class BoundVar<T> : IBoundVar<T>
	{
		private enum UpdateType { Inner, Outer };
		private sealed record Update(UpdateType Type, T Val);

		public Disp D { get; }
		public void Dispose() => D.Dispose();

		private readonly BehaviorSubject<T> Subj;
		private readonly ISubject<Update> whenUpdate;
		private IObservable<Update> WhenUpdate { get; }

		// IRoVar<T>
		// =========
		public IDisposable Subscribe(IObserver<T> observer) => Subj.Subscribe(observer);

		// IRwVar<T>
		// =========
		public T V
		{
			get => Subj.Value;
			set => SetOuter(value);
		}

		// IBoundVar<T>
		// ============
		public IObservable<T> WhenOuter => WhenUpdate.Where(e => e.Type == UpdateType.Outer).Select(e => e.Val);
		public IObservable<T> WhenInner => WhenUpdate.Where(e => e.Type == UpdateType.Inner).Select(e => e.Val);
		public void SetInner(T v) => whenUpdate.OnNext(new Update(UpdateType.Inner, v));
		private void SetOuter(T v) => whenUpdate.OnNext(new Update(UpdateType.Outer, v));

		public BoundVar(T init, Disp d)
		{
			D = d;
			Subj = new BehaviorSubject<T>(init).D(d);
			whenUpdate = new Subject<Update>().D(d);
			WhenUpdate = whenUpdate.AsObservable();
			WhenUpdate.Subscribe(e => Subj.OnNext(e.Val)).D(d);
		}
	}
}