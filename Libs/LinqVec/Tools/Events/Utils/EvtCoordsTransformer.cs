using System.Reactive.Linq;
using Geom;
using LinqVec.Structs;
using PowRxVar;

namespace LinqVec.Tools.Events.Utils;

public static class EvtCoordsTransformer
{
	public static IObservable<IEvt> ToGrid(this IObservable<IEvt> src, IRoVar<Transform> t)
		=> src.Transform(p => p.ToGrid(t.V));

	public static IObservable<IEvt> SnapToGrid(this IObservable<IEvt> src)
		=> src
			.Transform(p => p.SnapToGrid())
			.DistinctUntilChanged();

	private static Pt SnapToGrid(this Pt ptSrc) => new(
		MathF.Round(ptSrc.X),
		MathF.Round(ptSrc.Y)
	);


	private static IObservable<IEvt> Transform(this IObservable<IEvt> src, Func<Pt, Pt> fun) => src.Select(e => e.Transform(fun));

	private static IEvt Transform(this IEvt src, Func<Pt, Pt> fun) =>
		src switch
		{
			MouseMoveEvt { Pos: var pos } => new MouseMoveEvt(fun(pos)),
			MouseBtnEvt { Pos: var pos, UpDown: var upDown, Btn: var btn } => new MouseBtnEvt(fun(pos), upDown, btn),
			MouseClickEvt { Pos: var pos, Btn: var btn } => new MouseClickEvt(fun(pos), btn),
			MouseWheelEvt { Pos: var pos, Delta: var delta } => new MouseWheelEvt(fun(pos), delta),
			_ => src
		};
}