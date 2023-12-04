using System.Reactive;
using System.Reactive.Linq;
using LinqVec.Structs;
using LinqVec.Utils;
using PowRxVar;
using LinqVec.Utils.WinForms_;
using PowMaybe;

namespace LinqVec.Tools.Events;

public enum MouseBtn
{
	Left,
	Right,
	Middle
}

public enum UpDown
{
	Down,
	Up,
}

public interface IEvtGen<T>;
public sealed record MouseMoveEvtGen<T>(T Pos) : IEvtGen<T>;
public sealed record MouseBtnEvtGen<T>(T Pos, UpDown UpDown, MouseBtn Btn) : IEvtGen<T>;
public sealed record MouseWheelEvtGen<T>(T Pos, int Delta) : IEvtGen<T>;
public sealed record KeyEvtGen<T>(UpDown UpDown, Keys Key) : IEvtGen<T>;




public static class EvtUtils
{
	public static IObservable<IEvtGen<PtInt>> MakeForControl(
		Control ctrl,
		IObservable<Unit> whenRepeatLastMouseMove
	)
	{
		var whenMouseMove = ctrl.Events().MouseMove.Select(e => new MouseMoveEvtGen<PtInt>(e.ToPtInt()));
		var whenMouseDown = ctrl.Events().MouseDown.Select(e => new MouseBtnEvtGen<PtInt>(e.ToPtInt(), UpDown.Down, e.ToBtn()));
		var whenMouseUp = ctrl.Events().MouseUp.Select(e => new MouseBtnEvtGen<PtInt>(e.ToPtInt(), UpDown.Up, e.ToBtn()));
		var whenMouseWheel = ctrl.Events().MouseWheel.Select(e => new MouseWheelEvtGen<PtInt>(e.ToPtInt(), Math.Sign(e.Delta)));
		var whenKeyDown = ctrl.Events().KeyDown.Select(e => new KeyEvtGen<PtInt>(UpDown.Down, e.KeyCode));
		var whenKeyUp = ctrl.Events().KeyUp.Select(e => new KeyEvtGen<PtInt>(UpDown.Up, e.KeyCode));

		var whenMouseMoveRepeat = whenRepeatLastMouseMove.WithLatestFrom(whenMouseMove).Select(e => e.Second);

		return
			Obs.Merge<IEvtGen<PtInt>>(
				whenMouseMove,
				whenMouseDown,
				whenMouseUp,
				whenMouseWheel,
				whenKeyDown,
				whenKeyUp,
				whenMouseMoveRepeat
			)
				.MakeHot(ctrl);
	}

	public static IObservable<IEvtGen<PtInt>> RestrictToTool(
		this IObservable<IEvtGen<PtInt>> src,
		ITool tool,
		IRoVar<ITool> curTool,
		IRoVar<bool> isPanZoom
	)
	{

		//var isEvtOn = Var.Expr(() => curTool.V.IsSomeAndEqualTo(tool) && !isPanZoom.V);

		var isEvtOn = Var.Combine(curTool, isPanZoom, (cur, pan) => cur == tool && !pan);

		var whenMouseMove = src.WhenMouseMoveEvt();
		var whenMouseMoveRepeat = isEvtOn.WithLatestFrom(whenMouseMove).Select(e => e.Second);
		return src.Merge(
			whenMouseMoveRepeat
		)
			.Where(_ => isEvtOn.V);
	}
	private static IObservable<MouseMoveEvtGen<T>> WhenMouseMoveEvt<T>(this IObservable<IEvtGen<T>> src) => src.OfType<MouseMoveEvtGen<T>>();


	public static IObservable<IEvtGen<Pt>> ToGrid(this IObservable<IEvtGen<PtInt>> src, IRoVar<Transform> t)
		=> src.Transform(p => p.Scr2Grid(t.V));

	public static IObservable<IEvtGen<Maybe<Pt>>> SnapToGrid(this IObservable<IEvtGen<Pt>> src, IRoVar<Transform> t)
		=> src
			.Transform(p => p.SnapToGrid(t.V))
			.DistinctUntilChanged();

	public static IObservable<IEvtGen<Maybe<Pt>>> TrackPos(this IObservable<IEvtGen<Maybe<Pt>>> src, out IRoMayVar<Pt> mousePos, IRoDispBase d)
	{
		mousePos = VarMay.Make(
			src
				.OfType<MouseMoveEvtGen<Maybe<Pt>>>()
				.Select(e => e.Pos)
		).D(d);
		return src;
	}

	public static IObservable<IEvtGen<Pt>> RestrictToGrid(this IObservable<IEvtGen<Maybe<Pt>>> src) =>
		src
			.Where(e => e switch
			{
				MouseMoveEvtGen<Maybe<Pt>> { Pos: var mayPos } => mayPos.IsSome(),
				MouseBtnEvtGen<Maybe<Pt>> { Pos: var mayPos } => mayPos.IsSome(),
				MouseWheelEvtGen<Maybe<Pt>> { Pos: var mayPos } => mayPos.IsSome(),
				KeyEvtGen<Maybe<Pt>> => true,
				_ => throw new ArgumentException()
			})
			.Transform(p => p.Ensure());



	private static IObservable<IEvtGen<U>> Transform<T, U>(this IObservable<IEvtGen<T>> src, Func<T, U> fun) => src.Select(e => e.Transform(fun));

	private static IEvtGen<U> Transform<T, U>(this IEvtGen<T> src, Func<T, U> fun) =>
		src switch
		{
			MouseMoveEvtGen<T> { Pos: var pos } => new MouseMoveEvtGen<U>(fun(pos)),
			MouseBtnEvtGen<T> { Pos: var pos, UpDown: var upDown, Btn: var btn } => new MouseBtnEvtGen<U>(fun(pos), upDown, btn),
			MouseWheelEvtGen<T> { Pos: var pos, Delta: var delta } => new MouseWheelEvtGen<U>(fun(pos), delta),
			KeyEvtGen<T> { UpDown: var upDown, Key: var key } => new KeyEvtGen<U>(upDown, key),
			_ => throw new ArgumentException()
		};



	private static MouseBtn ToBtn(this MouseEventArgs evt)
	{
		if ((evt.Button & MouseButtons.Left) != 0) return MouseBtn.Left;
		if ((evt.Button & MouseButtons.Right) != 0) return MouseBtn.Right;
		if ((evt.Button & MouseButtons.Middle) != 0) return MouseBtn.Middle;
		throw new ArgumentException($"Invalid mouse buttons: {evt.Button}");
	}
}
