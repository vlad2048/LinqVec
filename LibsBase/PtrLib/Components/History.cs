using LanguageExt.UnitsOfMeasure;
using PowBasics.CollectionsExt;
using ReactiveVars;

namespace PtrLib.Components;

sealed class History<T>
{
	private readonly Stack<T> undos = new();
	private readonly Stack<T> redos = new();
	private readonly IBoundVar<T> cur;

	public History(IBoundVar<T> cur)
	{
		this.cur = cur;
		cur.WhenOuter.Subscribe(curV =>
		{
			undos.Push(curV);
			ClearRedos();
		}).D(cur.D);
	}

	public bool Undo()
	{
		if (!undos.TryPop(out var undoVal)) return false;
		redos.Push(cur.V);
		cur.SetInner(undoVal);
		return true;
	}

	public void Redo()
	{
		if (!redos.TryPop(out var redoVal)) return;
		undos.Push(cur.V);
		cur.SetInner(redoVal);
		cur.SetInner(redoVal);
	}

	public void ClearRedos() => redos.Clear();


	public void Add<U>(History<U> subHistory, Func<T, U, T> add, Func<U, bool> valid) =>
		subHistory.undos.Reverse().Append(subHistory.cur.V)
			.Where(valid)
			.Select(sub => add(cur.V, sub))
			.Prepend(cur.V)
			.DistinctUntilChangedSkipFirst()
			.ForEach(t => cur.V = t);
}



file static class HistoryEnumExt
{
	public static IEnumerable<T> DistinctUntilChangedSkipFirst<T>(this IEnumerable<T> source)
	{
		var arr = source as T[] ?? source.ToArray();
		return arr.Zip(arr.Skip(1))
			.Where(t => !t.Item1!.Equals(t.Item2))
			.Select(t => t.Item1);
	}
}