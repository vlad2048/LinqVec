using System.Reactive.Linq;
using Geom;
using ReactiveVars;

namespace LinqVec.Tools.Events;

public class Evt
{
	private readonly Action<Cursor> setCursor;

	public IObservable<IEvt> WhenEvt { get; }
	public IRoVar<bool> IsMouseDown { get; }
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
		WhenEvt = whenEvt
			//.Where(e => !IsMouseUp(e))
			.MakeHot(d);
		//IsMouseDown = WhenEvt.IsMouseDown();
		IsMouseDown =
			Obs.Merge(
					WhenEvt.WhenMouseDown().Select(_ => true),
					WhenEvt.WhenMouseUp().Select(_ => false),
					WhenEvt.OfType<MouseLeftBtnUpOutside>().Select(_ => false)
				)
				.Prepend(false)
				.ToVar(d);
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

	private static bool IsMouseUp(IEvt e) => e switch {
		MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left } => true,
		MouseLeftBtnUpOutside => true,
		_ => false
	};
}