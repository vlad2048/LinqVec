using System.Reactive.Linq;
using Geom;
using LinqVec.Utils.WinForms_;
using LinqVec.Utils.Rx;
using ReactiveUI;
using ReactiveVars;

namespace LinqVec.Tools.Events;

public static class EvtExt
{
	private static readonly TimeSpan KeyDelayStart = (SystemInformation.KeyboardDelay + 1) * TimeSpan.FromMilliseconds(250);
	private static readonly TimeSpan KeyDelayRepeat = TimeSpan.FromSeconds(1.0 / (2.5 + 27.5 * SystemInformation.KeyboardSpeed / 31.0));

	// Mouse Move
	// ==========
	public static bool IsMouseMove(this IEvt evt) => evt is MouseMoveEvt;
	public static bool IsMouseMove(this IEvt evt, out Pt pt)
	{
		if (evt is MouseMoveEvt { Pos: var pos })
		{
			pt = pos;
			return true;
		}
		else
		{
			pt = default;
			return false;
		}
	}
	public static IObservable<Pt> WhenMouseMove(this IObservable<IEvt> src) => src.OfType<MouseMoveEvt>().Select(e => e.Pos);
	public static IObservable<Pt> WhenMouseMove(this Evt src) => src.WhenEvt.WhenMouseMove();

	// Mouse Enter
	// ===========
	public static bool IsMouseEnter(this IEvt evt) => evt is MouseEnter;
	public static IObservable<Unit> WhenMouseEnter(this IObservable<IEvt> src) => src.OfType<MouseEnter>().ToUnit();

	// Mouse Leave
	// ===========
	public static bool IsMouseLeave(this IEvt evt) => evt is MouseLeave;
	public static IObservable<Unit> WhenMouseLeave(this IObservable<IEvt> src) => src.OfType<MouseLeave>().ToUnit();

	// Mouse Down
	// ==========
	public static bool IsMouseDown(this IEvt evt, MouseBtn btn = MouseBtn.Left) => evt is MouseBtnEvt { UpDown: UpDown.Down, Btn: var mouseBtn } && mouseBtn == btn;
	public static bool IsMouseDown(this IEvt evt, out Pt pt, MouseBtn btn = MouseBtn.Left)
	{
		if (evt is MouseBtnEvt { Pos: var pos, UpDown: UpDown.Down, Btn: var mouseBtn } && mouseBtn == btn)
		{
			pt = pos;
			return true;
		}
		else
		{
			pt = default;
			return false;
		}
	}
	public static IObservable<Pt> WhenMouseDown(this IObservable<IEvt> src, MouseBtn btn = MouseBtn.Left) =>
		src
			.OfType<MouseBtnEvt>()
			.Where(e => e is { UpDown: UpDown.Down, Btn: var mouseBtn } && mouseBtn == btn)
			.Select(e => e.Pos);
	public static IObservable<Pt> WhenMouseDown(this Evt src, MouseBtn btn = MouseBtn.Left) => src.WhenEvt.WhenMouseDown(btn);

	// Mouse Up
	// ========
	public static bool IsMouseUp(this IEvt evt, MouseBtn btn = MouseBtn.Left) => evt is MouseBtnEvt { UpDown: UpDown.Up, Btn: var mouseBtn } && mouseBtn == btn;
	public static bool IsMouseUp(this IEvt evt, out Pt pt, MouseBtn btn = MouseBtn.Left)
	{
		if (evt is MouseBtnEvt { Pos: var pos, UpDown: UpDown.Up, Btn: var mouseBtn } && mouseBtn == btn)
		{
			pt = pos;
			return true;
		}
		else
		{
			pt = default;
			return false;
		}
	}
	public static IObservable<Pt> WhenMouseUp(this IObservable<IEvt> src, MouseBtn btn = MouseBtn.Left) =>
		src
			.OfType<MouseBtnEvt>()
			.Where(e => e is { UpDown: UpDown.Up, Btn: var mouseBtn } && mouseBtn == btn)
			.Select(e => e.Pos);
	public static IObservable<Pt> WhenMouseUp(this Evt src, MouseBtn btn = MouseBtn.Left) => src.WhenEvt.WhenMouseUp(btn);

	// Mouse Wheel
	// ===========
	public static bool IsMouseWheel(this IEvt evt) => evt is MouseWheelEvt;
	public static IObservable<MouseWheelEvt> WhenMouseWheel(this IObservable<IEvt> src) =>
		src
			.OfType<MouseWheelEvt>();
	public static IObservable<MouseWheelEvt> WhenMouseWheel(this Evt src) => src.WhenEvt.WhenMouseWheel();

	// Key Down
	// ========
	public static bool IsKeyDown(this IEvt evt, Keys key, bool ctrl = false) => evt is KeyEvt { UpDown: UpDown.Down, Key: var evtKey } && evtKey == key && (!ctrl || KeyUtils.IsCtrlPressed);
	public static IObservable<Unit> WhenKeyDown(this IObservable<IEvt> src, Keys key, bool ctrl = false) =>
		src
			.OfType<KeyEvt>()
			.Where(e => e.UpDown == UpDown.Down && e.Key == key)
			.Where(_ => !ctrl || KeyUtils.IsCtrlPressed)
			.ToUnit();
	public static IObservable<Unit> WhenKeyDown(this Evt src, Keys key, bool ctrl = false) => src.WhenEvt.WhenKeyDown(key, ctrl);

	// Key Up
	// ======
	public static bool IsKeyUp(this IEvt evt, Keys key) => evt is KeyEvt { UpDown: UpDown.Up, Key: var evtKey } && evtKey == key;
	public static IObservable<Unit> WhenKeyUp(this IObservable<IEvt> src, Keys key) =>
		src
			.OfType<KeyEvt>()
			.Where(e => e.UpDown == UpDown.Up && e.Key == key)
			.ToUnit();
	public static IObservable<Unit> WhenKeyUp(this Evt src, Keys key) => src.WhenEvt.WhenKeyUp(key);

	// Is Key Down
	// ===========
	public static IRoVar<bool> IsKeyDown(this IObservable<IEvt> src, Keys key) =>
		Obs.Merge(
				src.WhenKeyDown(key).Select(_ => true),
				src.WhenKeyUp(key).Select(_ => false)
			)
			.Prepend(false)
			.DistinctUntilChanged()
			.ToVar();

	// Key Repeat
	// ==========
	/*public static IObservable<Unit> WhenKeyRepeat(this IObservable<IEvt> src, Keys key, bool ctrl)
	{
		var whenStart = src.WhenKeyDown(key).Where(_ => !ctrl || KeyUtils.IsCtrlPressed).ToUnit();
		var whenStop = src.OfType<KeyEvt>().ToUnit();
		return
			from _ in whenStart
			from evt in Obs.Timer(KeyDelayStart, KeyDelayRepeat, Rx.Sched).Prepend(0).Select(_ => Unit.Default).TakeUntil(whenStop)
			select evt;
	}*/

	// Mouse Down
	// ==========
	public static IRoVar<bool> IsMouseDown(this IObservable<IEvt> src, MouseBtn btn = MouseBtn.Left) =>
		Obs.Merge(
				src.WhenMouseDown(btn).Select(_ => true),
				src.WhenMouseUp(btn).Select(_ => false)
			)
			.Prepend(false)
			.ToVar();
}