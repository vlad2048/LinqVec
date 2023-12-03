using System.Reactive.Linq;
using LinqVec.Tools._Base.Events;
using PowRxVar;

namespace LinqVec.Tools.Curve_.Events;

interface ICurveEvt;

sealed record MoveCurveEvt(Pt Pos) : ICurveEvt;
sealed record ClickCurveEvt(Pt Pos) : ICurveEvt;
sealed record DragStartCurveEvt(Pt Pos) : ICurveEvt;
sealed record DragMoveCurveEvt(Pt PosDown, Pt Pos) : ICurveEvt;
sealed record DragEndCurveEvt(Pt PosDown, Pt Pos) : ICurveEvt;


static class CurveEvtExt
{
	private enum CurveState { Move, DragWait, Drag }

	public static IObservable<ICurveEvt> ToCurveEvt(
        this IObservable<IEvtGen<Pt>> src,
        IRoDispBase d
    ) =>
		Obs.Create<ICurveEvt>(obs =>
			{
				var obsD = new Disp();
				void Send(ICurveEvt evtDst) => obs.OnNext(evtDst);
				var curveStateRw = Var.Make(CurveState.Move).D(obsD);

				var downPos = Pt.Zero;
				src.Subscribe(evtSrc =>
				{
					switch (curveStateRw.V)
					{
						case CurveState.Move:
						{
							switch (evtSrc)
							{
								case MouseMoveEvtGen<Pt> { Pos: var pos }:
									Send(new MoveCurveEvt(pos));
									break;
								case MouseBtnEvtGen<Pt> { Pos: var pos, UpDown: UpDown.Down, Btn: MouseBtn.Left }:
									downPos = pos;
									curveStateRw.V = CurveState.DragWait;
									break;
							}

							break;
						}
						case CurveState.DragWait:
						{
							switch (evtSrc)
							{
								case MouseMoveEvtGen<Pt> { Pos: var pos } when pos != downPos:
									Send(new DragStartCurveEvt(downPos));
									Send(new DragMoveCurveEvt(downPos, pos));
									curveStateRw.V = CurveState.Drag;
									break;
								case MouseBtnEvtGen<Pt> { Pos: var pos, UpDown: UpDown.Up, Btn: MouseBtn.Left }:
									Send(new ClickCurveEvt(pos));
									curveStateRw.V = CurveState.Move;
									break;
							}

							break;
						}
						case CurveState.Drag:
						{
							switch (evtSrc)
							{
								case MouseMoveEvtGen<Pt> { Pos: var pos }:
									Send(new DragMoveCurveEvt(downPos, pos));
									break;
								case MouseBtnEvtGen<Pt> { Pos: var pos, UpDown: UpDown.Up, Btn: MouseBtn.Left }:
									Send(new DragEndCurveEvt(downPos, pos));
									curveStateRw.V = CurveState.Move;
									break;
							}

							break;
						}
					}
				}).D(obsD);

				return obsD;
			})
			.DistinctUntilChanged()
			.MakeHot(d);
}


/*
public static class RxLogExt
{
	public static IObservable<T> Conn<T>(this IConnectableObservable<T> obs, IRoDispBase d)
	{
		obs.Connect().D(d);
		return obs;
	}
}
*/