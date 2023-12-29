<Query Kind="Program">
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
</Query>

#load "_common\rx"

// System
using System.Threading.Tasks;

// Reactive
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using Obs = System.Reactive.Linq.Observable;
using Disp = System.Reactive.Disposables.CompositeDisposable;

// LanguageExt
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;

// ReactiveVars
using ReactiveVars;
using static ReactiveVars.DispMaker;

// LINQPad
using static RxTestMakers;



void Main()
{
	var doc = new Unmod<Doc>(Doc.Empty, D);
	using (var _ = doc.AddMod(new Mod<Doc>(true, Var.MakeConst<Func<Doc, Doc>>(doc => doc.AddCurve(new Curve(Guid.NewGuid(), ['A', 'B'])) )))) ;
	using (var _ = doc.AddMod(new Mod<Doc>(true, Var.MakeConst<Func<Doc, Doc>>(doc => doc.AddCurve(new Curve(Guid.NewGuid(), ['C', 'D'])) )))) ;

	//var curve = new Unmod<Curve>(Curve.Empty, D);
	var curveD = MkD().D(D);

	var curve = doc.SubCreate(Curve.Empty, SetCurve, e => e.Pts.Length > 1, curveD);

	void Log()
	{
		doc.Log();
		curve.Log();
		"".Dump();
	}
	
	Log();
	
	using (var _ = curve.AddMod(new Mod<Curve>(true, Var.MakeConst<Func<Curve, Curve>>(curve => curve.AddPt('D') )))) ;
	using (var _ = curve.AddMod(new Mod<Curve>(true, Var.MakeConst<Func<Curve, Curve>>(curve => curve.AddPt('E') )))) ;
	using (var _ = curve.AddMod(new Mod<Curve>(true, Var.MakeConst<Func<Curve, Curve>>(curve => curve.AddPt('F') )))) ;

	Log();
	
	doc.SubCommit(curve);

	doc.Log();
	doc.Undo().Dump("doc.Undo()");
	doc.Log();
	doc.Undo().Dump("doc.Redo()");
	doc.Log();
	doc.Undo().Dump("doc.Redo()");
	doc.Log();
	doc.Undo().Dump("doc.Redo()");
	doc.Log();
	doc.Undo().Dump("doc.Redo()");
	doc.Log();
}


public static Doc SetCurve(Doc doc, Curve curve) => doc with { Curves = doc.Curves.AddSet(curve) };




public sealed record Mod<T>(
	bool ApplyWhenDone,
	IRoVar<Func<T, T>> Fun
);



public interface IUnmod : IDisposable
{
	bool Undo();
	bool Redo();
}
public sealed class Unmod<T> : Undoer<T>, IUnmod
{
	private readonly List<Mod<T>> mods = new();	
	private void FlushModsAndClearRedos()
	{
		if (IsDisposed) throw new ArgumentException();
		foreach (var mod in mods)
			if (mod.ApplyWhenDone)	
				Cur.V = mod.Fun.V(Cur.V);
		mods.Clear();
		ClearRedos();
	}
	
	
	public Unmod(T init, Disp d) : base(init, d)
	{
	}

	public T VModded
	{
		get
		{
			if (IsDisposed) throw new ArgumentException();
			return mods.Aggregate(Cur.V, (acc, mod) => mod.Fun.V(acc));
		}
	}
	
	public IDisposable AddMod(Mod<T> mod)
	{
		if (IsDisposed) throw new ArgumentException();
		mods.Add(mod);
		return Disposable.Create(() =>
		{
			if (!mods.Contains(mod)) return;
			if (mod.ApplyWhenDone) Cur.V = mod.Fun.V(Cur.V);
			mods.Remove(mod);
		});
	}


	private sealed record SubWithCommit(IUnmod Sub, Action Commit);

	private Option<SubWithCommit> subMod = None;
	
	public Unmod<U> SubCreate<U>(
		U init,
		Func<T, U, T> setFun,
		Func<U, bool> validFun,
		Disp d
	)
	{
		if (IsDisposed) throw new ArgumentException();
		subMod.IfSome(_ => DisposeSub());
		var sub = new Unmod<U>(init, d);
		var commit = () =>
		{
			FlushModsAndClearRedos();
			sub.FlushModsAndClearRedos();
			foreach (var subVal in sub.StackUndoExt)
				if (validFun(subVal))
					Cur.V = setFun(Cur.V, subVal);
		};
		subMod = new SubWithCommit(sub, commit);
		return sub;
	}
	
	public void SubCancel<U>(Unmod<U> sub)
	{
		if (IsDisposed) throw new ArgumentException();
		if (subMod.Map(e => e.Sub) != sub) throw new ArgumentException();
		DisposeSub();
	}

	public void SubCommit<U>(Unmod<U> sub)
	{
		if (IsDisposed) throw new ArgumentException();
		if (subMod.Map(e => e.Sub) != sub) throw new ArgumentException();
		subMod.Ensure().Commit();
		DisposeSub();
	}

	public override bool Undo()
	{
		if (subMod.Match(e => e.Sub.Undo(), () => false))
			return true;
		else
			return base.Undo();
	}

	public override bool Redo()
	{
		if (subMod.Match(e => e.Sub.Redo(), () => false))
			return true;
		else
			return base.Redo();
	}




	private void DisposeSub()
	{
		if (IsDisposed) throw new ArgumentException();
		subMod.IfSome(e =>
		{
			D.Remove(e.Sub);
			e.Sub.Dispose();
		});
		subMod = None;
	}

	private const string ColUndo = "#1e7882";
	private const string ColCur = "#58c5d1";
	public void Log()
	{
		if (IsDisposed) throw new ArgumentException();
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
	}
}



public class Undoer<T> : IDisposable
{
	protected readonly Disp D;
	public bool IsDisposed { get; private set; }
	public void Dispose()
	{
		if (IsDisposed) return;
		IsDisposed = true;
		D.Dispose();
	}

	private readonly ISubject<Unit> whenUndo;
	private readonly ISubject<Unit> whenRedo;
	private readonly Stack<T> stackUndo = new();
	private readonly Stack<T> stackRedo = new();

	protected T[] StackUndoExt
	{
		get
		{
			if (IsDisposed) throw new ArgumentException();
			return stackUndo.Reverse().Append(Cur.V).ToArray();
		}
	}
	protected void ClearRedos()
	{
		if (IsDisposed) throw new ArgumentException();
		stackRedo.Clear();
	}

	private readonly IBoundVar<T> curV;
	public IBoundVar<T> Cur {
		get
		{
			if (IsDisposed) throw new ArgumentException();
			return curV;
		}
	}
	public virtual bool Undo()
	{
		if (IsDisposed) throw new ArgumentException();
		if (stackUndo.Count == 0) return false;
		whenUndo.OnNext(Unit.Default);
		return true;
	}
	public virtual bool Redo()
	{
		if (IsDisposed) throw new ArgumentException();
		if (stackRedo.Count == 0) return false;
		whenRedo.OnNext(Unit.Default);
		return true;
	}

	public Undoer(T init, Disp d)
	{
		this.D = d;
		curV = Var.MakeBound(init, d);
		whenUndo = new Subject<Unit>().D(d);
		whenRedo = new Subject<Unit>().D(d);

		var cur = Cur.V;

		Cur.WhenOuter.Subscribe(v =>
		{
			if (IsDisposed) throw new ArgumentException();
			stackUndo.Push(cur);
			stackRedo.Clear();
			cur = v;
		}).D(d);

		whenUndo
			.Subscribe(_ =>
			{
				if (IsDisposed) throw new ArgumentException();
				var valUndo = stackUndo.Pop();
				stackRedo.Push(Cur.V);
				Cur.SetInner(valUndo);
				cur = valUndo;
			}).D(d);

		whenRedo
			.Subscribe(_ =>
			{
				if (IsDisposed) throw new ArgumentException();
				var valRedo = stackRedo.Pop();
				stackUndo.Push(Cur.V);
				Cur.SetInner(valRedo);
				cur = valRedo;
			}).D(d);
	}
}




public static class DocExt
{
	public static Doc AddCurve(this Doc doc, Curve curve) => doc with { Curves = doc.Curves.AddArr(curve) };
	public static Curve AddPt(this Curve curve, char pt) => curve with { Pts = curve.Pts.AddArr(pt) };
}


public sealed record Doc(
	Curve[] Curves
)
{
	public override string ToString() => "[" + Curves.JoinText("; ") + "]";
	public static readonly Doc Empty = new([]);
	public static Doc Busy => new([new Curve(Guid.NewGuid(), ['A', 'B', 'C'])]);
}
public interface IId { Guid Id { get; } }
public sealed record Curve(Guid Id, char[] Pts) : IId
{
	public static Curve Empty => new(Guid.NewGuid(), []);
	public override string ToString() => $"Curve({Pts.Select(e => $"{e}").JoinText(",")})";
}



public static class EnumIdExts
{
	public static T[] AddSet<T>(this T[] xs, T x) where T : IId
	{
		var idx = xs.IndexOf(e => e.Id == x.Id);
		var list = xs.ToList();
		if (idx == -1) return list.Append(x).ToArray();
		list[idx] = x;
		return list.ToArray();
	}
}



public static class EnumExt
{
	public static T[] AddArrIf<T>(this T[] xs, bool condition, T x) => condition switch
	{
		false => xs,
		true => xs.AddArr(x)
	};
	public static T[] AddArr<T>(this T[] xs, T x) => xs.ToList().Append(x).ToArray();
	public static T[] RemoveArr<T>(this T[] xs, T x)
	{
		var list = xs.ToList();
		if (!list.Remove(x)) throw new ArgumentException();
		return list.ToArray();
	}
	public static T[] ToggleArr<T>(this T[] xs, T x) => xs.Contains(x) switch
	{
		false => xs.AddArr(x),
		true => xs.RemoveArr(x)
	};
	public static T[] SetIdxArr<T>(this T[] arr, int idx, Func<T, T> fun)
	{
		var list = arr.Take(idx).ToList();
		list.Add(fun(arr[idx]));
		list.AddRange(arr.Skip(idx + 1));
		return list.ToArray();
	}
}


public static class OptionExt
{
	public static T Ensure<T>(this Option<T> opt) => opt.IfNone(() => throw new ArgumentException());
}



































