using System.Reactive.Linq;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;
using UILib;

namespace LinqVec.Tools.Events.Utils;

static class EvtMaker
{
	public static IObservable<IEvt> MakeForControl(
		Control ctrl,
		IObservable<Unit> whenRepeatLastMouseMove
	)
	{
		var whenMouseMove = ctrl.Events().MouseMove.Select(e => new MouseMoveEvt(e.ToPt()));
		var whenMouseEnter = ctrl.Events().MouseEnter.Select(_ => new MouseEnterEvt());
		var whenMouseLeave = ctrl.Events().MouseLeave.Select(_ => new MouseLeaveEvt());
		var whenMouseDown = ctrl.Events().MouseDown.Select(e => new MouseBtnEvt(e.ToPt(), UpDown.Down, e.ToBtn(), ModKeyState.Make()));
		var whenMouseUp = ctrl.Events().MouseUp.Select(e => new MouseBtnEvt(e.ToPt(), UpDown.Up, e.ToBtn(), ModKeyState.Make()));
		var whenMouseWheel = ctrl.Events().MouseWheel.Select(e => new MouseWheelEvt(e.ToPt(), Math.Sign(e.Delta)));
		var whenKeyDown = ctrl.Events().KeyDown.Select(e => new KeyEvt(UpDown.Down, e.KeyCode));
		var whenKeyUp = ctrl.Events().KeyUp.Select(e => new KeyEvt(UpDown.Up, e.KeyCode));

		var whenMouseMoveRepeat = whenRepeatLastMouseMove
			//.Do(_ => LR.LogThread("whenRepeatLastMouseMove -> Triggered (1)"))
			.Delay(TimeSpan.Zero, Rx.Sched)
			//.Do(_ => LR.LogThread("whenRepeatLastMouseMove -> Triggered (2)"))
			.WithLatestFrom(whenMouseMove).Select(e => e.Second);
		var ctrlD = ctrl.GetD();
		return
			Obs.Merge<IEvt>(
					whenMouseMove, //.Do(_ => LR.LogThread("MouseMoveOut")),
					whenMouseEnter,
					whenMouseLeave,
					whenMouseDown,
					whenMouseUp,
					whenMouseWheel,
					whenKeyDown,
					whenKeyUp,
					whenMouseMoveRepeat //.Do(_ => LR.LogThread("MouseMoveOut (repeat)"))
				)
				.MakeHot(ctrlD);
	}


	private static MouseBtn ToBtn(this MouseEventArgs evt)
	{
		if ((evt.Button & MouseButtons.Left) != 0) return MouseBtn.Left;
		if ((evt.Button & MouseButtons.Right) != 0) return MouseBtn.Right;
		if ((evt.Button & MouseButtons.Middle) != 0) return MouseBtn.Middle;
		throw new ArgumentException($"Invalid mouse buttons: {evt.Button}");
	}
}