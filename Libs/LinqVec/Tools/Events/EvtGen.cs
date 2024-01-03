using System.Reactive.Linq;
using Geom;
using ReactiveVars;

namespace LinqVec.Tools.Events;

public static class EvtUtils
{
	public static Evt ToEvt(this IObservable<IEvt> src, Action<Cursor> setCursor, IObservable<Unit> whenUndoRedo, Disp d) =>
		new(
			src,
			setCursor,
			whenUndoRedo,
			d
		);

	public static IObservable<Pt> WhereSelectMousePos(this IObservable<IEvt> src) =>
		src
			.Where(e => e is MouseMoveEvt or MouseBtnEvt or MouseClickEvt or MouseWheelEvt)
			.Select(e => e switch
			{
				MouseMoveEvt { Pos: var pos } => pos,
				MouseBtnEvt { Pos: var pos } => pos,
				MouseClickEvt { Pos: var pos } => pos,
				MouseWheelEvt { Pos: var pos } => pos,
				_ => throw new ArgumentException()
			});




	public static IObservable<IEvt> RestrictToTool(
		this IObservable<IEvt> src,
		ITool tool,
		IRoVar<ITool> curTool,
		IRoVar<bool> isPanZoom
	)
	{
		var isEvtOn = Obs.CombineLatest(curTool, isPanZoom, (cur, pan) => cur == tool && !pan).ToVar();
		var whenMouseMove = src.WhenMouseMoveEvt();
		var whenMouseMoveRepeat = isEvtOn.WithLatestFrom(whenMouseMove).Select(e => e.Second);
		return src.Merge(
			whenMouseMoveRepeat
		)
			.Where(_ => isEvtOn.V);
	}
	private static IObservable<MouseMoveEvt> WhenMouseMoveEvt(this IObservable<IEvt> src) => src.OfType<MouseMoveEvt>();
}
