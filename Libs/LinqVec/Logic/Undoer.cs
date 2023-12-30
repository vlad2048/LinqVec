/*
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveVars;
using System.Reactive.Subjects;
using LinqVec.Utils.Rx;
using PowBasics.CollectionsExt;

namespace LinqVec.Logic;


//
//	Cur.WhenOuter	<=>	WhenDo
//	Cur.WhenInner	<=>	Obs.Merge(WhenUndo, WhenRedo)
//
public class Undoer<T> : IDisposable
{
	public Disp D { get; }
	public bool IsDisposed => D.IsDisposed;
	public void Dispose() => D.Dispose();

	private readonly ISubject<Unit> whenUndo;
	private readonly ISubject<Unit> whenRedo;
	private readonly Stack<T> stackUndo = new();
	private readonly Stack<T> stackRedo = new();
	private readonly ISubject<Unit> whenUncommittedDispose;
	private bool isCommitted;

	public void FlagIsCommitted() => isCommitted = true;
	public IObservable<Unit> WhenUncommittedDispose => whenUncommittedDispose.AsObservable();

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
	public IBoundVar<T> Cur
	{
		get
		{
			if (IsDisposed) throw new ArgumentException();
			return curV;
		}
	}

	public IObservable<Unit> WhenDo => Cur.WhenOuter.ToUnit();

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

	public IRoVar<T> CurReadOnly => Cur;

	public Undoer(T init, Disp d)
	{
		D = d;
		curV = Var.MakeBound(init, d);
		whenUndo = new Subject<Unit>().D(d);
		whenRedo = new Subject<Unit>().D(d);

		Disposable.Create(() =>
		{
			if (isCommitted) return;
			whenUncommittedDispose!.OnNext(Unit.Default);
			whenUncommittedDispose.OnCompleted();
		}).D(D);

		whenUncommittedDispose = new AsyncSubject<Unit>().D(d);

		var cur = Cur.V;

		Cur.WhenOuter.Subscribe(v =>
		{
			if (IsDisposed) throw new ArgumentException();
			stackUndo.Push(cur);
			ClearRedos();
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



	private const int DoCol = 0x42f55a;
	private const int UndoCol = 0xe32d49;
	private const int RedoCol = 0x782fde;
	private const int ArrowCol = 0xf2e33d;
	private const int StateCol = 0x167a72;
	private const int StateCurCol = 0x3dccc1;

	public IDisposable PrintLog(Func<T, string> fmt) =>
		Obs.Merge(
			Cur.WhenOuter.Select(_ => ("Do  ", DoCol)),
			whenUndo.Select(_ => ("Undo", UndoCol)),
			whenRedo.Select(_ => ("Redo", RedoCol))
		)
		.Subscribe(op =>
		{
			var all = stackUndo.Concat(stackRedo).Append(Cur.V).SelectToArray(fmt);
			var lng = all.Max(e => e.Length) + 1;
			var x = 8 + lng * stackUndo.Count + lng / 2 - 1;
			LC.WriteLine(new string(' ', x) + "↓", ArrowCol);
			LC.Write($"{op.Item1}    ", op.Item2);
			foreach (var elt in stackUndo.Rev())
				LC.Write(fmt(elt).PadRight(lng), StateCol);
			LC.Write(fmt(Cur.V).PadRight(lng), StateCurCol);
			foreach (var elt in stackRedo)
				LC.Write(fmt(elt).PadRight(lng), StateCol);
			L.WriteLine();
		}).D(D);
}
*/
