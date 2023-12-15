using Geom;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Events;
using LinqVec.Utils.Rx;
using PowRxVar;
using PowRxVar.Utils;
using UILib;

namespace LinqVec.Tools.Acts.Events;

interface IActEvt;

sealed record DragStartActEvt(Pt PtStart) : IActEvt
{
	public override string ToString() => $"DragStart({PtStart})";
}
sealed record DragEndActEvt(Pt PtStart, Pt PtEnd) : IActEvt
{
	public override string ToString() => $"DragEnd({PtStart}, {PtEnd})";
}
sealed record ClickActEvt(Pt Pt) : IActEvt
{
	public override string ToString() => $"Click({Pt})";
}
sealed record RightClickActEvt(Pt Pt) : IActEvt
{
	public override string ToString() => $"RightClick({Pt})";
}
sealed record DoubleClickActEvt(Pt Pt) : IActEvt
{
	public override string ToString() => $"DoubleClick({Pt})";
}
sealed record KeyDownActEvt(Keys Key) : IActEvt
{
	public override string ToString() => $"KeyDown({Key})";
}



static class ActEvtMaker
{
	private static readonly TimeSpan ClickDelay = TimeSpan.FromMilliseconds(500);

	private interface IState;
	private sealed record NoneState : IState;
	private sealed record WaitClickOrDragState(MouseBtn Btn, Pt Pos, DateTime Time) : IState;
	private sealed record WaitDoubleClickDownState : IState;
	private sealed record WaitDoubleClickUpState : IState;
	private sealed record WaitDragState(Pt PosStart) : IState;

	private static readonly TimeSpan ClickTime = TimeSpan.FromMilliseconds(500);


	public static IObservable<IActEvt> ToActEvt(this Evt evt, Gesture gestures, IRoDispBase d) =>
		Obs.Create<IActEvt>(obs =>
			{
				var obsD = new Disp();
				void Send(IActEvt evtDst) => obs.OnNext(evtDst);
				var state = Var.Make<IState>(new NoneState()).D(obsD);
				//var timeout = new EvtTimeout().D(obsD);

				var canDrag = gestures.HasFlag(Gesture.Drag);
				var canClick = gestures.HasFlag(Gesture.Click);
				var canRightClick = gestures.HasFlag(Gesture.RightClick);
				var canDoubleClick = gestures.HasFlag(Gesture.DoubleClick);

				evt.WhenEvt.Subscribe(e =>
				{

					switch (state.V)
					{
						case NoneState:
							switch (e)
							{
								case MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left, Pos: var mousePos } when canDrag || canClick || canDoubleClick:
									state.V = new WaitClickOrDragState(MouseBtn.Left, mousePos, DateTime.Now);
									break;
								case MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Right, Pos: var mousePos } when canRightClick:
									state.V = new WaitClickOrDragState(MouseBtn.Right, mousePos, DateTime.Now);
									break;
								default:
									break;
							}
							break;

						// (Click -> DoubleClick) | Drag
						// =============================
						case WaitClickOrDragState { Btn: MouseBtn.Left, Pos: var statePos, Time: var stateTime }:
							switch (e)
							{
								case MouseMoveEvt { Pos: var mousePos }:
									Send(new DragStartActEvt(statePos));
									state.V = new WaitDragState(statePos);
									break;
								case MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left, Pos: var mousePos } when canClick || canDoubleClick:
									if (canClick && DateTime.Now - stateTime < ClickDelay)
									{
										Send(new ClickActEvt(mousePos));
										state.V = new NoneState();
									}
									else
									{
										state.V = new NoneState();
									}
									break;
							}
							break;


						// Right Click
						// ===========
						case WaitClickOrDragState { Btn: MouseBtn.Right, Pos: var statePos, Time: var stateTime }:
							switch (e)
							{
								case MouseMoveEvt { Pos: var mousePos }:
									state.V = new NoneState();
									break;
								case MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Right, Pos: var mousePos }:
									if (DateTime.Now - stateTime < ClickDelay)
									{
										Send(new RightClickActEvt(mousePos));
										state.V = new NoneState();
									}
									else
									{
										state.V = new NoneState();
									}
									break;
							}
							break;

						case WaitDoubleClickDownState:
							break;

						case WaitDoubleClickUpState:
							break;

						case WaitDragState { PosStart: var statePosStart }:
							switch (e)
							{
								case MouseBtnEvt { Pos: var mousePos, Btn: MouseBtn.Left, UpDown: UpDown.Up }:
									Send(new DragEndActEvt(statePosStart, mousePos));
									state.V = new NoneState();
									break;
								default:
									break;
							}
							break;
					}



				}).D(obsD);

				return obsD;
			})
			.MakeHot(d);
}