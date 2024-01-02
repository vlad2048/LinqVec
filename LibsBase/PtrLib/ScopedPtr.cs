using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PtrLib.Components;
using ReactiveVars;

namespace PtrLib;

sealed class ScopedPtr<TSub> : IScopedPtr<TSub>
{
	private readonly Disp d = MkD($"ScopedPtr<{typeof(TSub).Name}>");
	private void EnsureNotDisp() => ObjectDisposedException.ThrowIf(d.IsDisposed, this);
	public void Dispose()
	{
		// we can when we close the doc
		//mod.V.IfSome(modV => throw new InvalidOperationException($"Cannot dispose ScopePtr<{typeof(TSub).Name}> while a Mod is active. Mod.Name: '{modV.Name}'"));
		whenFinished.OnNext(isCommited);
		whenFinished.OnCompleted();
		d.Dispose();
	}

	private readonly IBoundVar<TSub> v;
	private readonly IRwVar<TSub> vGfx;
	private readonly IRwVar<Option<Mod<TSub>>> mod;
	private readonly AsyncSubject<bool> whenFinished;
	private readonly Subject<IModEvt> whenModEvt;
	private bool isCommited;

	// @formatter:off
	public IRwVar<TSub> V { get { EnsureNotDisp(); return v; } }
	public IRoVar<TSub> VGfx { get { EnsureNotDisp(); return vGfx; } }
	// @formatter:on
	public History<TSub> History { get; }
	public IObservable<bool> WhenFinished => whenFinished.AsObservable();
	public void Commit()
	{
		EnsureNotDisp();
		isCommited = true;
		Dispose();
	}

	public ScopedPtr(
		TSub init
	)
	{
		v = Var.MakeBound(init, d);
		vGfx = init.Make(d);
		mod = Option<Mod<TSub>>.None.Make(d);
		whenFinished = new AsyncSubject<bool>().D(d);
		whenModEvt = new Subject<IModEvt>().D(d);
		History = new History<TSub>(v);

		mod
			.Select(e => e.Match(
				f => f.Fun.InterceptErrorAndCompletion(commit =>
					{
						//L.WriteLine($"Intercept: {commit}");
						if (commit)
							V.V = VGfx.V;
						else
							vGfx.V = V.V;
						whenModEvt.OnNext(new ModFinishEvt(f.Name, commit, $"{V.V}"));
						mod.V = None;
					}),
				Obs.Never<Func<TSub, TSub>>
			))
			.Switch()
			.Subscribe(fun => vGfx.V = fun(V.V)).D(d);
	}

	public void SetMod(Mod<TSub> modV)
	{
		EnsureNotDisp();
		if (mod.V.IsSome) throw new ObjectDisposedException("Previous mod should have finished first");
		whenModEvt.OnNext(new ModStartEvt(modV.Name));
		mod.V = modV;
	}

	public IObservable<Unit> WhenPaintNeeded => VGfx.ToUnit();
	public IObservable<IModEvt> WhenModEvt => whenModEvt.AsObservable();
}




file static class ScopedPtrRxExt
{
	public static IObservable<T> InterceptErrorAndCompletion<T>(this IObservable<T> source, Action<bool> action) =>
		source
			.Materialize()
			.Select(Some)
			.Select(e =>
			{
				var intercept = e.Match(
					f =>
					{
						var yes = f.Kind is NotificationKind.OnError or NotificationKind.OnCompleted;
						if (yes) action(f.Kind == NotificationKind.OnCompleted);
						return yes;
					},
					() => false
				);
				return intercept switch
				{
					false => e,
					true => None
				};
			})
			.WhereSome()
			.Dematerialize();



	private static IObservable<T> WhereSome<T>(this IObservable<Option<T>> src) =>
		src
			.Where(e => e.IsSome)
			.Select(e => e.Ensure());

	private static T Ensure<T>(this Option<T> opt) => opt.IfNone(() => throw new ArgumentException());
}