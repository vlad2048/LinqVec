using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Events.Utils;
using PowRxVar;

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

public interface IEvt;
public sealed record MouseMoveEvt(Pt Pos) : IEvt
{
	public override string ToString() => $"Move({Pos})";
}
public sealed record MouseEnter : IEvt
{
	public override string ToString() => "Enter";
}
public sealed record MouseLeave : IEvt
{
	public override string ToString() => "Leave";
}
public sealed record MouseBtnEvt(Pt Pos, UpDown UpDown, MouseBtn Btn) : IEvt
{
	public override string ToString() => $"{Btn}.{UpDown}({Pos})";
}
public sealed record MouseClickEvt(Pt Pos, MouseBtn Btn) : IEvt
{
	public override string ToString() => $"{Btn}.Click({Pos})";
}
public sealed record MouseWheelEvt(Pt Pos, int Delta) : IEvt
{
	public override string ToString() => $"Wheel({Pos}, delta={Delta})";
}
public sealed record KeyEvt(UpDown UpDown, Keys Key) : IEvt
{
	public override string ToString() => $"Key.{UpDown}({Key})";
}



public class Evt : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Action<Cursor> setCursor;

	public IObservable<IEvt> WhenEvt { get; }
	public void SetCursor(Cursor cursor) => setCursor(cursor);
	public IRoVar<Option<Pt>> MousePos { get; }

	public Evt(
		IObservable<IEvt> whenEvt,
		Action<Cursor> setCursor
	)
	{
		WhenEvt = whenEvt.MakeHot(d);
		this.setCursor = setCursor;
		MousePos = WhenEvt.TrackMouse().D(d);
	}
}



public static class EvtUtils
{
	public static Evt ToEvt(this IObservable<IEvt> src, Action<Cursor> setCursor, IRoDispBase d) => new Evt(src, setCursor).D(d);

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
		var isEvtOn = Var.Combine(curTool, isPanZoom, (cur, pan) => cur == tool && !pan);
		var whenMouseMove = src.WhenMouseMoveEvt();
		var whenMouseMoveRepeat = isEvtOn.WithLatestFrom(whenMouseMove).Select(e => e.Second);
		return src.Merge(
			whenMouseMoveRepeat
		)
			.Where(_ => isEvtOn.V);
	}
	private static IObservable<MouseMoveEvt> WhenMouseMoveEvt(this IObservable<IEvt> src) => src.OfType<MouseMoveEvt>();





	/*public static IObservable<IEvtGen<Maybe<Pt>>> TrackPos(this IObservable<IEvtGen<Maybe<Pt>>> src, out IRoMayVar<Pt> mousePos, IRoDispBase d)
	{
		mousePos = VarMay.Make(
			src
				.OfType<MouseMoveEvtGen<Maybe<Pt>>>()
				.Select(e => e.Pos)
		).D(d);
		return src;
	}

	public static IObservable<IEvt> TrackPos(this IObservable<IEvt> src, out IRoMayVar<Pt> mousePos, IRoDispBase d)
	{
		mousePos = VarMay.Make(
			src
				.OfType<MouseMoveEvtGen<Maybe<Pt>>>()
				.Select(e => e.Pos)
		).D(d);
		return src;
	}

	public static IObservable<IEvt> RestrictToGrid(this IObservable<IEvtGen<Maybe<Pt>>> src) =>
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
			.Transform(p => p.Ensure());*/
}
