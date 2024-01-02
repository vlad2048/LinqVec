using System.Reactive.Linq;
using PtrLib.Components;
using ReactiveVars;

namespace PtrLib;




sealed class Ptr<TDoc> : IPtr<TDoc>
{
	private readonly Disp d;
	private bool IsDisposed => d.IsDisposed;
	public void Dispose() => d.Dispose();

	private readonly IRwVar<TDoc> vGfx;
	private readonly History<TDoc> history;
	private readonly IRwVar<Option<IScopedPtr>> scope;

	public IBoundVar<TDoc> V { get; }
	public IRoVar<TDoc> VGfx => vGfx;

	public Ptr(TDoc init, Disp d)
	{
		this.d = d;
		V = Var.MakeBound(init, d);
		vGfx = Var.Make(init, d);
		history = new History<TDoc>(V);
		scope = Option<IScopedPtr>.None.Make(d);
	}

	public IScopedPtr<TSub> Scope<TSub>(
		TSub init,
		Func<TDoc, TSub, TDoc> del,
		Func<TDoc, TSub, TDoc> add,
		Func<TSub, bool> valid
	)
	{
		if (scope.V.IsSome) throw new ObjectDisposedException("Previous scope should have been disposed first");
		var scopeV = new ScopedPtr<TSub>(init);

		// ScopeStart
		vGfx.V = del(VGfx.V, scopeV.V.V);

		scopeV.WhenFinished.Subscribe(isCommited =>
		{
			if (isCommited)
			{
				// ScopeCommit
				V.V = add(V.V, scopeV.V.V);
				history.Add(scopeV.History, add, valid);
			}
			else
			{
				// ScopeCancel
			}
			// Always
			if (IsDisposed) return;
			vGfx.V = V.V;
			scope.V = None;
		});

		scope.V = scopeV;
		return scopeV;
	}

	public void Undo() { }
	public void Redo() { }

	public IObservable<Unit> WhenPaintNeeded =>
		Obs.Merge(
			VGfx.ToUnit(),
			scope.Select(subOpt => subOpt.Match(
				sub => sub.WhenPaintNeeded,
				Obs.Never<Unit>
			)).Switch()
		);
}
