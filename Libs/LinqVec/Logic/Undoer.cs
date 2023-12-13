using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Utils.Rx;
using PowRxVar;

namespace LinqVec.Logic;


public interface IUndoer : IDisposable
{
    IObservable<Unit> WhenDo { get; }
    IObservable<Unit> WhenChanged { get; }
	bool Undo();
	bool Redo();
	void InvalidateRedos();
}

sealed class UndoMan : IUndoer, IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly IUndoer docUndoer;
	private readonly IRwVar<IUndoer> toolUndoer;

	public IObservable<Unit> WhenDo => Obs.Merge(docUndoer.WhenDo, toolUndoer.Select(e => e.WhenDo).Switch());
	public IObservable<Unit> WhenChanged => Obs.Merge(docUndoer.WhenChanged, toolUndoer.Select(e => e.WhenChanged).Switch());

	public bool Undo() => toolUndoer.V.Undo() || docUndoer.Undo();
	public bool Redo() => toolUndoer.V.Redo() || docUndoer.Redo();

	public void InvalidateRedos()
	{
		docUndoer.InvalidateRedos();
		toolUndoer.V.InvalidateRedos();
	}

	public void SetToolUndoer(IUndoer toolUndoer_) => toolUndoer.V = toolUndoer_;

	public UndoMan(IUndoer docUndoer)
	{
		this.docUndoer = docUndoer;
		toolUndoer = Var.Make(Undoer.Empty).D(d);
		toolUndoer.Select(e => e.WhenDo).Switch().Subscribe(_ => docUndoer.InvalidateRedos()).D(d);
	}
}


public static class Undoer
{
    public static readonly IUndoer Empty = new EmptyUndoer();

    private sealed class EmptyUndoer : IUndoer
    {
		public void Dispose() {}
	    public IObservable<Unit> WhenDo => Obs.Never<Unit>();
	    public IObservable<Unit> WhenChanged => Obs.Never<Unit>();
	    public bool Undo() => false;
	    public bool Redo() => false;
	    public void InvalidateRedos() {}
    }
}

sealed class Undoer<T> : IUndoer
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly IRwVar<T> cur;
    private readonly Stack<T> stackUndo = new();
    private readonly Stack<T> stackRedo = new();

    private readonly ISubject<T> whenDo;
    private readonly ISubject<Unit> whenUndo;
    private readonly ISubject<Unit> whenRedo;
    private IObservable<T> WhenDoT => whenDo.AsObservable();
    private IObservable<Unit> WhenUndo => whenUndo.AsObservable();
    private IObservable<Unit> WhenRedo => whenRedo.AsObservable();
    private void Do(T v) => whenDo.OnNext(v);

    public T V
    {
        get => cur.V;
        set => Do(value);
    }

	// IUndoer
    public IObservable<Unit> WhenDo => WhenDoT.ToUnitExt();
	public IObservable<Unit> WhenChanged => cur.AsObservable().ToUnitExt();
    public bool Undo()
    {
	    if (stackUndo.Count == 0) return false;
	    whenUndo.OnNext(Unit.Default);
	    return true;
    }
    public bool Redo()
    {
	    if (stackRedo.Count == 0) return false;
	    whenRedo.OnNext(Unit.Default);
	    return true;
    }
    public void InvalidateRedos() => stackRedo.Clear();



	public Undoer(T init)
    {
		whenDo = new Subject<T>().D(d);
	    whenUndo = new Subject<Unit>().D(d);
	    whenRedo = new Subject<Unit>().D(d);

		cur = Var.Make(init).D(d);

		WhenDoT.Subscribe(v =>
		{
			stackUndo.Push(cur.V);
			stackRedo.Clear();
			cur.V = v;
		}).D(d);

		WhenUndo.Subscribe(_ =>
		{
			if (stackUndo.Count == 0) throw new ArgumentException();
			var valUndo = stackUndo.Pop();
			stackRedo.Push(cur.V);
			cur.V = valUndo;
		}).D(d);

		WhenRedo.Subscribe(_ =>
		{
			if (stackRedo.Count == 0) throw new ArgumentException();
			var valRedo = stackRedo.Pop();
			stackUndo.Push(cur.V);
			cur.V = valRedo;
		}).D(d);
	}
}