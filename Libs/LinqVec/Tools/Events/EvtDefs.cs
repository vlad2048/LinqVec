using Geom;

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

public sealed record MouseEnterEvt : IEvt
{
	public override string ToString() => "Enter";
}

public sealed record MouseLeaveEvt : IEvt
{
	public override string ToString() => "Leave";
}

public sealed record MouseBtnEvt(Pt Pos, UpDown UpDown, MouseBtn Btn, ModKeyState ModKey) : IEvt
{
	public override string ToString() => $"{Btn}.{UpDown}({Pos})";
}

public sealed record MouseLeftBtnUpOutside : IEvt
{
	public override string ToString() => "LeftBtnUpOutside";
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



static class EvtStorybookSamples
{
	private static readonly Pt p0 = new(-10, -10);
	private static readonly Pt p1 = new(2, 3);
	private static readonly Pt p2 = new(-5, 2);

	public static IEvt[] Samples = [
		new MouseMoveEvt(p0),
		new MouseEnterEvt(),
		new MouseLeaveEvt(),
		new MouseBtnEvt(p1, UpDown.Down, MouseBtn.Left, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Up, MouseBtn.Left, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Down, MouseBtn.Right, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Up, MouseBtn.Right, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Down, MouseBtn.Middle, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Up, MouseBtn.Middle, ModKeyState.Empty),
		new MouseWheelEvt(p2, +1),
		new MouseWheelEvt(p2, -1),
		new KeyEvt(UpDown.Down, Keys.S),
		new KeyEvt(UpDown.Up, Keys.S),
	];
}