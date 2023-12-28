using ReactiveVars;
using System.Reactive.Linq;
using Geom;
using LinqVec.Utils;
using LinqVec.Utils.Rx;

namespace LinqVec.Logic;

public sealed record Mod<T>(
	string Name,
	bool ApplyWhenDone,
	IRoVar<Func<T, T>> Fun
)
{
	public static readonly Mod<T> Empty = new("Empty", false, Var.MakeConst<Func<T, T>>(e => e));
}



public static class UnmodExt
{
	public static Action ClearMod<O>(this Unmod<O> unmod) => () => unmod.ModSet(Mod<O>.Empty);
	public static Action HoverMod<O>(this Unmod<O> unmod, Mod<O> mod) => () => unmod.ModSet(mod);
	public static Action<Pt> DragMod<O>(this Unmod<O> unmod, Func<Pt, Mod<O>> mod) => pt => unmod.ModSet(mod(pt));
}


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
	public IObservable<Unit> WhenUndoRedo => Obs.Merge(
		Cur.WhenInner.ToUnit(),
		subMod
			.Select(e => e.Match(
				f => f.Sub.WhenUndoRedo,
				() => Obs.Return(Unit.Default)
			))
			.Switch()
	);

	public IObservable<Unit> WhenPaintNeeded => Obs.Merge(
		Cur.ToUnit(),
		mod
			.Select(e => e.Match(
				f => f.Fun.ToUnit(),
				() => Obs.Return(Unit.Default)
			))
			.Switch(),
		subMod
			.Select(e => e.Match(
				f => f.Sub.WhenPaintNeeded,
				() => Obs.Return(Unit.Default)
			))
			.Switch()
	);

	public Unmod(T init, Disp d) : base(init, d)
	{
		mod = Option<Mod<T>>.None.MakeSafe(d);
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
			//return mods.Aggregate(Cur.V, (acc, mod) => mod.Fun.V(acc));
		}
	}


	// *******
	// * Mod *
	// *******
	private readonly IRwVar<Option<Mod<T>>> mod;

	public void ModSet(Mod<T> mod_)
	{
		ModFlush();
		//if (!whenModEvt.IsDisposed) whenModEvt.OnNext(new SetModEvt(mod_.Name));
		mod.V = Some(mod_);
	}
	private void ModFlush()
	{
		mod.V.IfSome(m =>
		{
			if (m.ApplyWhenDone)
			{
				//if (!whenModEvt.IsDisposed) whenModEvt.OnNext(new ApplyModEvt(m.Name));
				Cur.V = m.Fun.V(Cur.V);
			}
			mod.V = None;
		});
	}

	private void FlushModAndClearRedos()
	{
		if (IsDisposed) throw new ArgumentException();
		ModFlush();
		ClearRedos();
	}


	// *******
	// * Sub *
	// *******
	private sealed record SubWithCommit(IUnmod Sub, Action Commit);

	private readonly IRwVar<Option<SubWithCommit>> subMod;

	public Unmod<U> SubCreate<U>(
		U init,
		Func<T, U, T> setFun,
		Func<U, bool> validFun,
		Disp d
	)
	{
		if (IsDisposed) throw new ArgumentException();
		subMod.V.IfSome(_ => DisposeSub());
		var sub = new Unmod<U>(init, d);
		var commit = () =>
		{
			FlushModAndClearRedos();
			sub.FlushModAndClearRedos();
			foreach (var subVal in sub.StackUndoExt)
				if (validFun(subVal))
					Cur.V = setFun(Cur.V, subVal);
		};
		subMod.V = new SubWithCommit(sub, commit);

		sub.WhenUncommittedDispose.Subscribe(_ => DisposeSubIf(sub)).D(d);

		return sub;
	}

	public void SubCancel<U>(Unmod<U> sub)
	{
		if (IsDisposed) throw new ArgumentException();
		if (subMod.V.Map(e => e.Sub) != sub) throw new ArgumentException();
		DisposeSub();
		//whenPaintNeeded.OnNext(Unit.Default);
	}

	public void SubCommit<U>(Unmod<U> sub)
	{
		if (IsDisposed) throw new ArgumentException();
		if (subMod.V.Map(e => e.Sub) != sub) throw new ArgumentException();
		var subModV = subMod.V.Ensure();
		subModV.Sub.FlagIsCommitted();
		subModV.Commit();
		DisposeSub();
		//whenPaintNeeded.OnNext(Unit.Default);
	}

	private void DisposeSub()
	{
		if (IsDisposed) throw new ArgumentException();
		subMod.V.IfSome(e =>
		{
			e.Sub.Dispose();
		});
		subMod.V = None;
	}

	private void DisposeSubIf<U>(Unmod<U> unmod)
	{
		if (IsDisposed) throw new ArgumentException();
		if (subMod.V.Map(e => e.Sub) == unmod)
			DisposeSub();
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

		/*if (subMod.V.Match(e => e.Sub.Redo(), () => false))
			return true;
		else
			return base.Redo();*/
	}






	

	/*private const string ColUndo = "#1e7882";
	private const string ColCur = "#58c5d1";
	public void Log()
	{
		var spans = new List<Span>();
		void Write(string str, string color)
		{
			var span = new Span(str);
			span.Styles["color"] = color;
			spans.Add(span);
		}
		var arr = StackUndoExt;
		if (arr.Length > 1)
			Write(arr.SkipLast().JoinText(" / ") + " / ", ColUndo);
		Write($"{arr.Last()}", ColCur);
		var div = new Div(spans);
		div.Dump();
	}*/
}
