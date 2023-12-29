using ReactiveVars;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PtrLib.Components;

/*
	Cur.WhenOuter	<=>	WhenDo
	Cur.WhenInner	<=>	Obs.Merge(WhenUndo, WhenRedo)
*/
sealed class Undoer<T> : IDisposable
{
	public Disp D { get; }
	private void EnsureNotDisp() => ObjectDisposedException.ThrowIf(IsDisposed, this);
	public bool IsDisposed => D.IsDisposed;
	public void Dispose() => D.Dispose();

	private readonly IBoundVar<T> curV;
	private readonly ISubject<Unit> whenUndo;
	private readonly ISubject<Unit> whenRedo;
	private readonly Stack<T> stackUndo = new();
	private readonly Stack<T> stackRedo = new();
	private readonly ISubject<Unit> whenUncommittedDispose;
	private bool isCommitted;

	internal T[] StackRedo { get { EnsureNotDisp(); return stackRedo.ToArray(); } }

	// @formatter:off
	public void FlagIsCommitted() => isCommitted = true;
	public IObservable<Unit> WhenUncommittedDispose => whenUncommittedDispose.AsObservable();
	public T[] StackUndoExt { get { EnsureNotDisp(); return stackUndo.Reverse().Append(Cur.V).ToArray(); } }
	public void ClearRedos() { EnsureNotDisp(); stackRedo.Clear(); }
	public IBoundVar<T> Cur { get { EnsureNotDisp(); return curV; } }
	public IObservable<Unit> WhenDo => Cur.WhenOuter.ToUnit();
	public bool Undo() { EnsureNotDisp(); if (stackUndo.Count == 0) return false; whenUndo.OnNext(Unit.Default); return true; }
	public bool Redo() { EnsureNotDisp(); if (stackRedo.Count == 0) return false; whenRedo.OnNext(Unit.Default); return true; }
	// @formatter:on

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
			EnsureNotDisp();
			stackUndo.Push(cur);
			ClearRedos();
			cur = v;
		}).D(d);

		whenUndo
			.Subscribe(_ =>
			{
				EnsureNotDisp();
				var valUndo = stackUndo.Pop();
				stackRedo.Push(Cur.V);
				Cur.SetInner(valUndo);
				cur = valUndo;
			}).D(d);

		whenRedo
			.Subscribe(_ =>
			{
				EnsureNotDisp();
				var valRedo = stackRedo.Pop();
				stackUndo.Push(Cur.V);
				Cur.SetInner(valRedo);
				cur = valRedo;
			}).D(d);
	}
}
