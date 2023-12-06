using PowRxVar;
using System.Reactive.Linq;
using System.Reactive;
using LinqVec.Utils.WinForms_;

namespace LinqVec.Tools.Events;

public static class EvtExt
{
	private static readonly TimeSpan KeyDelayStart = (SystemInformation.KeyboardDelay + 1) * TimeSpan.FromMilliseconds(250);
	private static readonly TimeSpan KeyDelayRepeat = TimeSpan.FromSeconds(1.0 / (2.5 + 27.5 * SystemInformation.KeyboardSpeed / 31.0));

	// Mouse Move
	// ==========
	public static bool IsMouseMove<T>(this IEvtGen<T> evt) => evt is MouseMoveEvtGen<T>;
	public static bool IsMouseMove<T>(this IEvtGen<T> evt, out T pt)
	{
		if (evt is MouseMoveEvtGen<T> { Pos: var pos })
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
	public static IObservable<T> WhenMouseMove<T>(this IObservable<IEvtGen<T>> src) => src.OfType<MouseMoveEvtGen<T>>().Select(e => e.Pos);
	public static IObservable<Pt> WhenMouseMove(this Evt src) => src.WhenEvt.WhenMouseMove();

	// Mouse Down
	// ==========
	public static bool IsMouseDown<T>(this IEvtGen<T> evt, MouseBtn btn = MouseBtn.Left) => evt is MouseBtnEvtGen<T> { UpDown: UpDown.Down, Btn: var mouseBtn } && mouseBtn == btn;
	public static bool IsMouseDown<T>(this IEvtGen<T> evt, out T pt, MouseBtn btn = MouseBtn.Left)
	{
		if (evt is MouseBtnEvtGen<T> { Pos: var pos, UpDown: UpDown.Down, Btn: var mouseBtn } && mouseBtn == btn)
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
	public static IObservable<T> WhenMouseDown<T>(this IObservable<IEvtGen<T>> src, MouseBtn btn = MouseBtn.Left) =>
		src
			.OfType<MouseBtnEvtGen<T>>()
			.Where(e => e is { UpDown: UpDown.Down, Btn: var mouseBtn } && mouseBtn == btn)
			.Select(e => e.Pos);
	public static IObservable<Pt> WhenMouseDown(this Evt src, MouseBtn btn = MouseBtn.Left) => src.WhenEvt.WhenMouseDown(btn);

	// Mouse Up
	// ========
	public static bool IsMouseUp<T>(this IEvtGen<T> evt, MouseBtn btn = MouseBtn.Left) => evt is MouseBtnEvtGen<T> { UpDown: UpDown.Up, Btn: var mouseBtn } && mouseBtn == btn;
	public static bool IsMouseUp<T>(this IEvtGen<T> evt, out T pt, MouseBtn btn = MouseBtn.Left)
	{
		if (evt is MouseBtnEvtGen<T> { Pos: var pos, UpDown: UpDown.Up, Btn: var mouseBtn } && mouseBtn == btn)
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
	public static IObservable<T> WhenMouseUp<T>(this IObservable<IEvtGen<T>> src, MouseBtn btn = MouseBtn.Left) =>
		src
			.OfType<MouseBtnEvtGen<T>>()
			.Where(e => e is { UpDown: UpDown.Up, Btn: var mouseBtn } && mouseBtn == btn)
			.Select(e => e.Pos);
	public static IObservable<Pt> WhenMouseUp(this Evt src, MouseBtn btn = MouseBtn.Left) => src.WhenEvt.WhenMouseUp(btn);

	// Mouse Wheel
	// ===========
	public static bool IsMouseWheel<T>(this IEvtGen<T> evt) => evt is MouseWheelEvtGen<T>;
	public static IObservable<MouseWheelEvtGen<T>> WhenMouseWheel<T>(this IObservable<IEvtGen<T>> src) =>
		src
			.OfType<MouseWheelEvtGen<T>>();
	public static IObservable<MouseWheelEvtGen<Pt>> WhenMouseWheel(this Evt src) => src.WhenEvt.WhenMouseWheel();

	// Key Down
	// ========
	public static bool IsKeyDown(this IEvtGen<Pt> evt, Keys key) => evt is KeyEvtGen<Pt> { UpDown: UpDown.Down, Key: var evtKey } && evtKey == key;
	public static IObservable<Unit> WhenKeyDown<T>(this IObservable<IEvtGen<T>> src, Keys key) =>
		src
			.OfType<KeyEvtGen<T>>()
			.Where(e => e.UpDown == UpDown.Down && e.Key == key)
			.ToUnit();
	public static IObservable<Unit> WhenKeyDown(this Evt src, Keys key) => src.WhenEvt.WhenKeyDown(key);

	// Key Up
	// ======
	public static bool IsKeyUp(this IEvtGen<Pt> evt, Keys key) => evt is KeyEvtGen<Pt> { UpDown: UpDown.Up, Key: var evtKey } && evtKey == key;
	public static IObservable<Unit> WhenKeyUp<T>(this IObservable<IEvtGen<T>> src, Keys key) =>
		src
			.OfType<KeyEvtGen<T>>()
			.Where(e => e.UpDown == UpDown.Up && e.Key == key)
			.ToUnit();
	public static IObservable<Unit> WhenKeyUp(this Evt src, Keys key) => src.WhenEvt.WhenKeyUp(key);

	// Is Key Down
	// ===========
	public static IRoVar<bool> IsKeyDown<T>(this IObservable<IEvtGen<T>> src, Keys key, IRoDispBase d) =>
		Var.Make(
			false,
			Observable.Merge(
				src.WhenKeyDown(key).Select(_ => true),
				src.WhenKeyUp(key).Select(_ => false)
			)
		).D(d);

	// Key Repeat
	// ==========
	public static IObservable<Unit> WhenKeyRepeat<T>(this IObservable<IEvtGen<T>> src, Keys key, bool ctrl)
	{
		var whenStart = src.WhenKeyDown(key).Where(_ => !ctrl || KeyUtils.IsCtrlPressed).ToUnit();
		var whenStop = src.OfType<KeyEvtGen<T>>().ToUnit();
		return
			from _ in whenStart
			from evt in Obs.Timer(KeyDelayStart, KeyDelayRepeat).Prepend(0).Select(_ => Unit.Default).TakeUntil(whenStop)
			select evt;
	}
}