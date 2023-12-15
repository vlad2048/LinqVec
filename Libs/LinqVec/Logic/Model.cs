using System.Reactive.Disposables;
using System.Reactive.Linq;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils.Rx;
using PowRxVar;

namespace LinqVec.Logic;


public sealed class Model<D> : IUndoer where D : IDoc
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly IObservable<IEvt> whenEvt;
	private readonly Undoer<D> undoer;
	private readonly IRwVar<bool> enableRedrawOnMouseMove;

	// IUndoer
	// =======
	public IObservable<Unit> WhenDo => undoer.WhenDo;
	public IObservable<Unit> WhenUndo => undoer.WhenUndo;
	public IObservable<Unit> WhenRedo => undoer.WhenRedo;
	public bool Undo() => undoer.Undo();
	public bool Redo() => undoer.Redo();
	public void InvalidateRedos() => undoer.InvalidateRedos();
	public IObservable<Unit> WhenChanged => undoer.WhenChanged;
	public string GetLogStr() => undoer.GetLogStr();

	public Model(D init, IObservable<IEvt> whenEvt)
	{
		this.whenEvt = whenEvt;
		undoer = new Undoer<D>(init).D(d);
		enableRedrawOnMouseMove = Var.Make(false).D(d);
	}

	public IObservable<Unit> WhenPaintNeeded => Obs.Merge(
		WhenChanged,
		enableRedrawOnMouseMove
			.Select(enabled => enabled switch
			{
				true => whenEvt.WhenMouseMove().ToUnitExt(),
				false => Obs.Never<Unit>()
			})
			.Switch(),
		whenEvt.WhenMouseLeave()
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
