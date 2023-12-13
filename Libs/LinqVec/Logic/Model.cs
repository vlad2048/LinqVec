using System.Reactive.Disposables;
using System.Reactive.Linq;
using LinqVec.Structs;
using PowRxVar;

namespace LinqVec.Logic;


public sealed class Model<D> : IUndoer where D : IDoc
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly IObservable<Unit> whenMouseMove;
	private readonly Undoer<D> undoer;
	private readonly IRwVar<bool> enableRedrawOnMouseMove;

	// IUndoer
	// =======
	public IObservable<Unit> WhenDo => undoer.WhenDo;
	public bool Undo() => undoer.Undo();
	public bool Redo() => undoer.Redo();
	public void InvalidateRedos() => undoer.InvalidateRedos();
	public IObservable<Unit> WhenChanged => undoer.WhenChanged;

	public Model(D init, IObservable<Unit> whenMouseMove)
	{
		this.whenMouseMove = whenMouseMove;
		undoer = new Undoer<D>(init).D(d);
		enableRedrawOnMouseMove = Var.Make(false).D(d);
	}

	public IObservable<Unit> WhenPaintNeeded => Obs.Merge(
		WhenChanged,
		enableRedrawOnMouseMove
			.Select(enabled => enabled switch
			{
				true => whenMouseMove,
				false => Obs.Never<Unit>()
			})
			.Switch()
	);

	public D V
	{
		get => undoer.V;
		set => undoer.V = value;
	}

	public void EnableRedrawOnMouseMove(IRoDispBase enableD)
	{
		enableRedrawOnMouseMove.V = true;
		Disposable.Create(() =>
		{
			if (!enableRedrawOnMouseMove.IsDisposed)
				enableRedrawOnMouseMove.V = false;
		}).D(enableD);
	}
}
