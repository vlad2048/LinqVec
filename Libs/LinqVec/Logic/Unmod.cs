using ReactiveVars;
using System.Reactive.Linq;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using LinqVec.Logic.Structs;

namespace LinqVec.Logic;


public interface IUnmod : IDisposable
{
	void FlagIsCommitted();
	bool Undo();
	bool Redo();
	IObservable<Unit> WhenDo { get; }
	IObservable<Unit> WhenUndoRedo { get; }
	IObservable<Unit> WhenPaintNeeded { get; }
}



public sealed class Unmod<T> : Undoer<T>, IUnmod
{
	private sealed record SubWithCommit(IUnmod Sub, Action Commit);

	private readonly IRwVar<Option<Mod<T>>> mod;
	private readonly IRwVar<Option<SubWithCommit>> subMod;

	public IObservable<Unit> WhenUndoRedo => UnmodUtils.CombineWhenUndoRedo(this, subMod.Map2(e => e.Sub));
	public IObservable<Unit> WhenPaintNeeded => UnmodUtils.CombineWhenPaintNeeded(this, subMod.Map2(e => e.Sub), mod);


	public Unmod(T init, Disp d) : base(init, d)
	{
		mod = Option<Mod<T>>.None.Make(d);
		subMod = Option<SubWithCommit>.None.Make(d);

		subMod
			.Select(e => e.Match(
				f => f.Sub.WhenDo,
				Obs.Never<Unit>
			))
			.Switch()
			.Subscribe(_ => ClearRedos()).D(d);
	}

	public T VModded
	{
		get
		{
			if (IsDisposed) throw new ArgumentException();
			return mod.V.Match(
				m => m.Fun.V(Cur.V),
				() => Cur.V
			);
		}
	}


	// *******
	// * Mod *
	// *******
	public void ModSet(Mod<T> mod_)
	{
		if (IsDisposed) throw new ArgumentException();
		ModFlush();
		mod.V = Some(mod_);
	}
	public void ModFlush()
	{
		if (IsDisposed) throw new ArgumentException();
		mod.V.IfSome(m =>
		{
			if (m.ApplyWhenDone)
			{
				Cur.V = m.Fun.V(Cur.V);
			}
			mod.V = None;
		});
	}



	// *******
	// * Sub *
	// *******
	public Unmod<U> SubCreate<U>(
		U init,
		Func<T, U, T> setFun,
		Func<U, bool> validFun,
		Disp d
	)
	{
		if (IsDisposed) throw new ArgumentException();
		var schedD = Rx.MkUID(d);
		subMod.V.IfSome(_ => DisposeSub());
		var sub = new Unmod<U>(init, schedD);

		void Commit()
		{
			FlushModAndClearRedos();
			sub.FlushModAndClearRedos();
			foreach (var subVal in sub.StackUndoExt)
				if (validFun(subVal))
					Cur.V = setFun(Cur.V, subVal);
		}

		subMod.V = new SubWithCommit(sub, Commit);

		sub.WhenUncommittedDispose
			.Where(_ => subMod.V.Map(e => e.Sub) == sub)
			.Subscribe(_ => DisposeSub()).D(schedD);

		return sub;
	}

	public void SubCancel<U>(Unmod<U> sub)
	{
		if (IsDisposed) throw new ArgumentException();
		if (subMod.V.Map(e => e.Sub) != sub) throw new ArgumentException();
		DisposeSub();
	}

	public void SubCommit<U>(Unmod<U> sub)
	{
		if (IsDisposed) throw new ArgumentException();
		if (subMod.V.Map(e => e.Sub) != sub) throw new ArgumentException();
		var subModV = subMod.V.Ensure();
		subModV.Sub.FlagIsCommitted();
		subModV.Commit();
		DisposeSub();
	}

	private void FlushModAndClearRedos()
	{
		if (IsDisposed) throw new ArgumentException();
		ModFlush();
		ClearRedos();
	}
	
	private void DisposeSub()
	{
		if (IsDisposed) throw new ArgumentException();
		subMod.V.IfSome(e => e.Sub.Dispose());
		subMod.V = None;
	}



	public override bool Undo()
	{
		if (subMod.V.Match(e => e.Sub.Undo(), () => false))
			return true;
		else
			return base.Undo();
	}

	public override bool Redo()
	{
		if (base.Redo())
			return true;
		else
			return subMod.V.Match(e => e.Sub.Redo(), () => false);
	}
}







file static class UnmodUtils
{
	public static IObservable<Unit> CombineWhenUndoRedo<T>(
		Unmod<T> dad,
		IObservable<Option<IUnmod>> kid
	) =>
		Obs.Merge(
			dad.Cur.WhenInner.ToUnit(),
			kid
				.Select(e => e.Match(
					f => f.WhenUndoRedo,
					() => Obs.Return(Unit.Default)
				))
				.Switch()
		);

	public static IObservable<Unit> CombineWhenPaintNeeded<T>(
		Unmod<T> dad,
		IObservable<Option<IUnmod>> kid,
		IObservable<Option<Mod<T>>> mod
	) =>
		Obs.Merge(
			dad.Cur.ToUnit(),
			mod
				.Select(e => e.Match(
					f => f.Fun.ToUnit(),
					() => Obs.Return(Unit.Default)
				))
				.Switch(),
			kid
				.Select(e => e.Match(
					f => f.WhenPaintNeeded,
					() => Obs.Return(Unit.Default)
				))
				.Switch()
		);
}
