using System.Reactive.Linq;
using ReactiveVars;
using System.Reactive.Subjects;
using System.Text;
using PowBasics.CollectionsExt;

namespace LinqVec.Logic;


/*
	Cur.WhenOuter	<=>	WhenDo
	Cur.WhenInner	<=>	Obs.Merge(WhenUndo, WhenRedo)
*/
public sealed class Undoer<T> : IDisposable
{
	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly ISubject<Unit> whenUndo;
	private readonly ISubject<Unit> whenRedo;
	private readonly Stack<T> stackUndo = new();
	private readonly Stack<T> stackRedo = new();

	public IBoundVar<T> Cur { get; }
	public void Undo()
	{
		if (stackUndo.Count == 0) return;
		whenUndo.OnNext(Unit.Default);
	}

	public void Redo()
	{
		if (stackRedo.Count == 0) return;
		whenRedo.OnNext(Unit.Default);
	}

	public Undoer(T init, Disp d)
	{
		this.d = d;
		Cur = Var.MakeBound(init, d);
		whenUndo = new Subject<Unit>().D(d);
		whenRedo = new Subject<Unit>().D(d);

		var cur = Cur.V;

		Cur.WhenOuter.Subscribe(v =>
		{
			stackUndo.Push(cur);
			stackRedo.Clear();
			cur = v;
		}).D(d);

		whenUndo
			.Subscribe(_ =>
			{
				var valUndo = stackUndo.Pop();
				stackRedo.Push(Cur.V);
				Cur.SetInner(valUndo);
				cur = valUndo;
			}).D(d);

		whenRedo
			.Subscribe(_ =>
			{
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
			L.WriteLine(new string(' ', x) + "↓", ArrowCol);
			L.Write($"{op.Item1}    ", op.Item2);
			foreach (var elt in stackUndo.Rev())
				L.Write(fmt(elt).PadRight(lng), StateCol);
			L.Write(fmt(Cur.V).PadRight(lng), StateCurCol);
			foreach (var elt in stackRedo)
				L.Write(fmt(elt).PadRight(lng), StateCol);
			L.WriteLine();
		}).D(d);
}
