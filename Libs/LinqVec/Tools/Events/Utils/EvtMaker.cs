using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LinqVec.Utils;
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
		var whenMouseEnter = ctrl.Events().MouseEnter.Select(_ => new MouseEnter());
		var whenMouseLeave = ctrl.Events().MouseLeave.Select(_ => new MouseLeave());
		var whenMouseDown = ctrl.Events().MouseDown.Select(e => new MouseBtnEvt(e.ToPt(), UpDown.Down, e.ToBtn()));
		var whenMouseUp = ctrl.Events().MouseUp.Select(e => new MouseBtnEvt(e.ToPt(), UpDown.Up, e.ToBtn()));
		var whenMouseWheel = ctrl.Events().MouseWheel.Select(e => new MouseWheelEvt(e.ToPt(), Math.Sign(e.Delta)));
		var whenKeyDown = ctrl.Events().KeyDown.Select(e => new KeyEvt(UpDown.Down, e.KeyCode));
		var whenKeyUp = ctrl.Events().KeyUp.Select(e => new KeyEvt(UpDown.Up, e.KeyCode));

		var whenMouseMoveRepeat = whenRepeatLastMouseMove
			//.Delay(TimeSpan.Zero, new SynchronizationContextScheduler(SynchronizationContext.Current!))
			.Delay(TimeSpan.Zero)
			.WithLatestFrom(whenMouseMove).Select(e => e.Second);

		return
			Obs.Merge<IEvt>(
					whenMouseMove,
					whenMouseEnter,
					whenMouseLeave,
					whenMouseDown,
					whenMouseUp,
					whenMouseWheel,
					whenKeyDown,
					whenKeyUp,
					whenMouseMoveRepeat
				)
				//.SynthesizeClicks()
				.MakeHot(ctrl);
	}


	private static MouseBtn ToBtn(this MouseEventArgs evt)
	{
		if ((evt.Button & MouseButtons.Left) != 0) return MouseBtn.Left;
		if ((evt.Button & MouseButtons.Right) != 0) return MouseBtn.Right;
		if ((evt.Button & MouseButtons.Middle) != 0) return MouseBtn.Middle;
		throw new ArgumentException($"Invalid mouse buttons: {evt.Button}");
	}
}