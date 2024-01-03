using System.Reactive.Linq;
using Geom;
using ReactiveVars;

namespace LinqVec.Tools.Events;

public class Evt : IDisposable
{
	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly Action<Cursor> setCursor;

	public IObservable<IEvt> WhenEvt { get; }
	public IRwVar<bool> IsDragging { get; }
	public void SetCursor(Cursor? cursor)
	{
		if (cursor != null)
			setCursor(cursor);
	}
	public IRoVar<Option<Pt>> MousePos { get; }
	public IObservable<Unit> WhenUndoRedo { get; }

	public Evt(
		IObservable<IEvt> whenEvt,
		Action<Cursor> setCursor,
		IObservable<Unit> whenUndoRedo,
		Disp d
	)
	{
		this.d = d;
		WhenEvt = whenEvt.MakeHot(d);
		IsDragging = Var.Make(false, d);
		this.setCursor = setCursor;
		MousePos = Var.MakeOptionalFromOptionalObs(
			Obs.Merge(
				WhenEvt.WhenMouseMove().Select(Some),
				WhenEvt.WhenMouseLeave().Select(_ => Option<Pt>.None)
			),
			d
		);
		WhenUndoRedo = whenUndoRedo;
	}
}