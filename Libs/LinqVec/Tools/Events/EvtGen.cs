using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LinqVec.Structs;
using LinqVec.Utils;
using PowRxVar;
using LinqVec.Utils.WinForms_;
using PowMaybe;
using PowRxVar.Utils;

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

public sealed record MouseMoveEvtGen<T>(T Pos) : IEvtGen<T>
{
	public override string ToString() => $"Move({Pos})";
}
public sealed record MouseBtnEvtGen<T>(T Pos, UpDown UpDown, MouseBtn Btn) : IEvtGen<T>
{
	public override string ToString() => $"{Btn}.{UpDown}({Pos})";
}
public sealed record MouseClickEvtGen<T>(T Pos, MouseBtn Btn) : IEvtGen<T>
{
	public override string ToString() => $"{Btn}.Click({Pos})";
}
public sealed record MouseWheelEvtGen<T>(T Pos, int Delta) : IEvtGen<T>
{
	public override string ToString() => $"Wheel({Pos}, delta={Delta})";
}
public sealed record KeyEvtGen<T>(UpDown UpDown, Keys Key) : IEvtGen<T>
{
	public override string ToString() => $"Key.{UpDown}({Key})";
}



public class Evt
{
	private readonly Action<Cursor> setCursor;

	public IObservable<IEvtGen<Pt>> WhenEvt { get; }
	public void SetCursor(Cursor cursor) => setCursor(cursor);

	public Evt(IObservable<IEvtGen<Pt>> whenEvt, Action<Cursor> setCursor)
	{
		WhenEvt = whenEvt;
		this.setCursor = setCursor;
	}
}



public static class EvtUtils
{
	public static Evt ToEvt(this IObservable<IEvtGen<Pt>> src, Action<Cursor> setCursor) => new(src, setCursor);

	public static Maybe<T> GetMayMousePos<T>(this IEvtGen<T> e) =>
		e switch
		{
			MouseMoveEvtGen<T> { Pos: var pos } => May.Some(pos),
			MouseBtnEvtGen<T> { Pos: var pos } => May.Some(pos),
			MouseClickEvtGen<T> { Pos: var pos } => May.Some(pos),
			MouseWheelEvtGen<T> { Pos: var pos } => May.Some(pos),
			_ => May.None<T>()
		};

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

		var whenMouseMoveRepeat = whenRepeatLastMouseMove
			.Delay(TimeSpan.Zero, new SynchronizationContextScheduler(SynchronizationContext.Current!))
			.WithLatestFrom(whenMouseMove).Select(e => e.Second);

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
				//.SynthesizeClicks()
				.MakeHot(ctrl);
	}

	public static IObservable<IEvtGen<T>> NoClicks<T>(this IObservable<IEvtGen<T>> src) =>
		src
			.Select(e => e switch
			{
				//MouseClickEvtGen<T> { Btn: var btn, Pos: var pos } => new IEvtGen<T>[]
				//{
				//	new MouseBtnEvtGen<T>(pos, UpDown.Down, btn),
				//	new MouseBtnEvtGen<T>(pos, UpDown.Up, btn),
				//}.ToObservable(),
				MouseClickEvtGen<T> { Btn: var btn, Pos: var pos } =>
					Obs.Return(new MouseBtnEvtGen<T>(pos, UpDown.Down, btn)).Concat(
						Obs.Return(new MouseBtnEvtGen<T>(pos, UpDown.Up, btn)).Delay(TimeSpan.FromMilliseconds(100), new SynchronizationContextScheduler(SynchronizationContext.Current!))
					),
				_ => Obs.Return(e)
			})
			.Merge();

	private interface ISynth;
	private sealed record NoneSynth : ISynth;
	private sealed record ClickSynth(MouseBtn Btn, PtInt Pos) : ISynth;

	private static readonly TimeSpan ClickTime = TimeSpan.FromMilliseconds(500);

	private static IObservable<IEvtGen<PtInt>> SynthesizeClicks(
		this IObservable<IEvtGen<PtInt>> src
	) =>
		Obs.Create<IEvtGen<PtInt>>(obs =>
		{
			var obsD = new Disp();
			void Send(IEvtGen<PtInt> evtDst) => obs.OnNext(evtDst);
			var state = Var.Make<ISynth>(new NoneSynth()).D(obsD);

			var (timeout, whenTimeout) = RxEventMaker.Make<Unit>().D(obsD);
			var timeD = new SerialDisp<IRwDispBase>().D(obsD);
			timeD.Value = new Disp();
			void TimeoutSched() => Obs.Timer(ClickTime/*, new SynchronizationContextScheduler(SynchronizationContext.Current!)*/).Subscribe(_ => timeout(Unit.Default)).D(timeD.Value);
			void TimeoutCancel()
			{
				timeD.Value = null;
				timeD.Value = new Disp();
			}

			whenTimeout
				.ObserveOnUI()
				.Subscribe(_ =>
			{
				if (state.V is ClickSynth { Btn: var stateBtn, Pos: var statePos})
					Send(new MouseBtnEvtGen<PtInt>(statePos, UpDown.Down, stateBtn));
				TimeoutCancel();
				state.V = new NoneSynth();
			}).D(obsD);

			src.Subscribe(evtSrc =>
			{
				switch (state.V)
				{
					case NoneSynth:
						switch (evtSrc)
						{
							case MouseBtnEvtGen<PtInt> { UpDown: UpDown.Down, Btn: var btn, Pos: var pos }:
								state.V = new ClickSynth(btn, pos);
								TimeoutSched();
								break;
							default:
								Send(evtSrc);
								break;
						}
						break;

					case ClickSynth { Btn: var stateBtn, Pos: var statePos }:
						switch (evtSrc)
						{
							case MouseBtnEvtGen<PtInt> { UpDown: UpDown.Up, Btn: var btn, Pos: var pos } when btn == stateBtn:
								Send(new MouseClickEvtGen<PtInt>(statePos, stateBtn));
								break;
							default:
								Send(new MouseBtnEvtGen<PtInt>(statePos, UpDown.Down, stateBtn));
								Send(evtSrc);
								break;
						}
						TimeoutCancel();
						state.V = new NoneSynth();
						break;
				}
			}).D(obsD);

			return obsD;
		});

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
				MouseClickEvtGen<Maybe<Pt>> { Pos: var mayPos } => mayPos.IsSome(),
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
			MouseClickEvtGen<T> { Pos: var pos, Btn: var btn } => new MouseClickEvtGen<U>(fun(pos), btn),
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
