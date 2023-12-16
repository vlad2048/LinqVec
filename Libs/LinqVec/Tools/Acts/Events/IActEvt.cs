using Geom;

namespace LinqVec.Tools.Acts.Events;

interface IActEvt;

sealed record DragStartActEvt(Pt PtStart) : IActEvt
{
	public override string ToString() => $"DragStart({PtStart})";
}
sealed record DragEndActEvt(Pt PtStart, Pt PtEnd) : IActEvt
{
	public override string ToString() => $"DragEnd({PtStart}, {PtEnd})";
}
sealed record ClickActEvt(Pt Pt) : IActEvt
{
	public override string ToString() => $"Click({Pt})";
}
sealed record RightClickActEvt(Pt Pt) : IActEvt
{
	public override string ToString() => $"RightClick({Pt})";
}
sealed record DoubleClickActEvt(Pt Pt) : IActEvt
{
	public override string ToString() => $"DoubleClick({Pt})";
}
sealed record KeyDownActEvt(Keys Key) : IActEvt
{
	public override string ToString() => $"KeyDown({Key})";
}