using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveVars;

public static class Var
{
	public static IRoVar<T> MakeConst<T>(T val) => Obs.Return(val).ToVar();
	public static IRoVar<T> ToVar<T>(this IObservable<T> obs) => new RoVar<T>(obs);
	public static IRoVar<T> ToVar<T>(this IObservable<T> obs, Disp d) => obs.MakeReplay(d).ToVar();
	public static IRwVar<T> Make<T>(this T init, Disp d) => new RwVar<T>(init, false).D(d);
	public static IRwVar<T> MakeSafe<T>(this T init, Disp d) => new RwVar<T>(init, true).D(d);
	public static IBoundVar<T> MakeBound<T>(T init, Disp d) => new BoundVar<T>(init).D(d);

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


	private sealed class RwVar<T> : IRwVar<T>, IDisposable
	{
		public void Dispose() => Subj.Dispose();
		public IDisposable Subscribe(IObserver<T> observer) => Subj.Subscribe(observer);

		private readonly BehaviorSubject<T> Subj;
		private readonly bool safe;

		public T V
		{
			get => Subj.Value;
			set
			{
				if (Subj.IsDisposed && safe) return;
				Subj.OnNext(value);
			}
		}

		public void SetSafe(T v) => Subj.OnNext(v);


		public bool IsDisposed => Subj.IsDisposed;

		public RwVar(T init, bool safe)
		{
			this.safe = safe;
			Subj = new BehaviorSubject<T>(init);
		}
	}


	private sealed class BoundVar<T> : IBoundVar<T>, IDisposable
	{
		private enum UpdateType { Inner, Outer };
		private sealed record Update(UpdateType Type, T Val);

		private readonly Disp d = MkD();
		public void Dispose() => d.Dispose();

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
		public bool IsDisposed => Subj.IsDisposed;
		public void SetSafe(T v) => SetOuter(v);

		// IBoundVar<T>
		// ============
		public IObservable<T> WhenOuter => WhenUpdate.Where(e => e.Type == UpdateType.Outer).Select(e => e.Val);
		public IObservable<T> WhenInner => WhenUpdate.Where(e => e.Type == UpdateType.Inner).Select(e => e.Val);
		public void SetInner(T v) => whenUpdate.OnNext(new Update(UpdateType.Inner, v));
		private void SetOuter(T v) => whenUpdate.OnNext(new Update(UpdateType.Outer, v));

		public BoundVar(T init)
		{
			Subj = new BehaviorSubject<T>(init).D(d);
			whenUpdate = new Subject<Update>().D(d);
			WhenUpdate = whenUpdate.AsObservable();
			WhenUpdate.Subscribe(e => Subj.OnNext(e.Val)).D(d);
		}
	}
}