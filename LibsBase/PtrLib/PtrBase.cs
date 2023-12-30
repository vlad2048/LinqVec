using System.Reactive.Disposables;
using System.Reactive.Linq;
using PtrLib.Components;
using PtrLib.Utils;
using ReactiveVars;

namespace PtrLib;


class PtrBase<T> : IHasDisp
{
	public Disp D { get; }
	protected void EnsureNotDisp() => ObjectDisposedException.ThrowIf(IsDisposed, this);
	public bool IsDisposed => D.IsDisposed;
	public void Dispose() => D.Dispose();

	private readonly IRwVar<Option<Mod<T>>> mod;

	internal Undoer<T> Undoer { get; }

	// @formatter:off
	public T V { get { EnsureNotDisp(); return Undoer.Cur.V; } set { EnsureNotDisp(); Undoer.Cur.V = value; } }
	public virtual T VModded { get { EnsureNotDisp(); return V.Apply(mod); } }
	// @formatter:on

	public PtrBase(T init, Disp d)
	{
		D = d;
		Undoer = new Undoer<T>(init, D);
		mod = Option<Mod<T>>.None.Make(D);
	}

	public IDisposable ModSet(Mod<T> modVal)
	{
		EnsureNotDisp();

		if (mod.V.IsSome) throw new ObjectDisposedException("Previous mod should have been disposed first");
		/*mod.V.IfSome(modV =>
		{
			if (modV.Apply)
				V = V.Apply(modV);
		});*/


		mod.V = modVal;
		return Disposable.Create(() =>
		{
			if (IsDisposed) return;

			// This should never happen
			if (mod.V != modVal) throw new ObjectDisposedException("Mod has been changed before it could be disposed");
			if (modVal.Apply)
				V = V.Apply(modVal);
			mod.V = None;
		});
	}

	public IObservable<Unit> WhenPtrBasePaintNeeded =>
		Obs.Merge([
			Undoer.Cur.ToUnit(),
			mod
				.Select(e => e.Match(
					f => f.Fun.ToUnit(),
					() => Obs.Return(Unit.Default)
				))
				.Switch(),
		]);
}


