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
