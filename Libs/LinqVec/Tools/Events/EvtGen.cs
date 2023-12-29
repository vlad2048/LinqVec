using System.Reactive.Linq;
using Geom;
using LanguageExt.Pretty;
using LinqVec.Tools.Events.Utils;
using ReactiveVars;

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
public sealed record MouseBtnEvt(Pt Pos, UpDown UpDown, MouseBtn Btn, ModKeyState ModKey) : IEvt
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
	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly Action<Cursor> setCursor;

	public IObservable<IEvt> WhenEvt { get; }
	public void SetCursor(Cursor? cursor)
	{
		if (cursor != null)
			setCursor(cursor);
	}

	public IRoVar<Option<Pt>> MousePos { get; }
	public IObservable<Unit> WhenUndoRedo { get; }

	public Evt(
		IObservable<IEvt> whenEvt,
		Action<Cursor> setCursor,
		IObservable<Unit> whenUndoRedo,
		Disp d
	)
	{
		this.d = d;
		WhenEvt = whenEvt.MakeHot(d);
		this.setCursor = setCursor;
		MousePos = WhenEvt.TrackMouse(d);
		WhenUndoRedo = whenUndoRedo;
	}
}



public static class EvtUtils
{
	public static Evt ToEvt(this IObservable<IEvt> src, Action<Cursor> setCursor, IObservable<Unit> whenUndoRedo, Disp d) =>
		new(
			src,
			setCursor,
			whenUndoRedo,
			d
		);

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




	public static IObservable<IEvt> RestrictToTool<TDoc>(
		this IObservable<IEvt> src,
		ITool<TDoc> tool,
		IRoVar<ITool<TDoc>> curTool,
		IRoVar<bool> isPanZoom
	)
	{
		var isEvtOn = Obs.CombineLatest(curTool, isPanZoom, (cur, pan) => cur == tool && !pan).ToVar();
		var whenMouseMove = src.WhenMouseMoveEvt();
		var whenMouseMoveRepeat = isEvtOn.WithLatestFrom(whenMouseMove).Select(e => e.Second);
		return src.Merge(
			whenMouseMoveRepeat
		)
			.Where(_ => isEvtOn.V);
	}
	private static IObservable<MouseMoveEvt> WhenMouseMoveEvt(this IObservable<IEvt> src) => src.OfType<MouseMoveEvt>();
}
