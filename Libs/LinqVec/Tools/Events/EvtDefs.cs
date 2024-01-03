using Geom;
using LinqVec.Logging;
using LogLib.Interfaces;

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


public interface IEvt : IWriteSer;


public sealed record MouseMoveEvt(Pt Pos) : IEvt
{
	public override string ToString() => $"Move({Pos})";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

public sealed record MouseEnterEvt : IEvt
{
	public override string ToString() => "Enter";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

public sealed record MouseLeaveEvt : IEvt
{
	public override string ToString() => "Leave";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

public sealed record MouseBtnEvt(Pt Pos, UpDown UpDown, MouseBtn Btn, ModKeyState ModKey) : IEvt
{
	public override string ToString() => $"{Btn}.{UpDown}({Pos})";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

public sealed record MouseClickEvt(Pt Pos, MouseBtn Btn) : IEvt
{
	public override string ToString() => $"{Btn}.Click({Pos})";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

public sealed record MouseWheelEvt(Pt Pos, int Delta) : IEvt
{
	public override string ToString() => $"Wheel({Pos}, delta={Delta})";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

public sealed record KeyEvt(UpDown UpDown, Keys Key) : IEvt
{
	public override string ToString() => $"Key.{UpDown}({Key})";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}
