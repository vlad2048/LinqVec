using System.Reactive.Linq;
using PtrLib;
using ReactiveVars;

namespace LinqVec.Structs;

public interface IDocHolder
{
	// LayoutPane
	//IObservable<object> WhenChanged { get; }

	// VecEditor
	Action Undo { get; }
	Action Redo { get; }
	IObservable<Unit> WhenUndoRedo { get; }
	IObservable<Unit> WhenPaintNeeded { get; }
}

public sealed record DocHolder(
	//IObservable<object> WhenChanged,
	Action Undo,
	Action Redo,
	IObservable<Unit> WhenUndoRedo,
	IObservable<Unit> WhenPaintNeeded
) : IDocHolder
{
	public static DocHolder Make<T>(IPtr<T> ptr) => new(

		//ptr.V.WhenOuter.Select(_ => (object)ptr.V),
		ptr.Undo,
		ptr.Redo,
		ptr.V.WhenInner.ToUnit(),
		ptr.WhenPaintNeeded
	);
}