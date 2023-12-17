using Geom;

namespace LinqVec.Tools.Acts.Events;

interface IActEvt;

enum ConfirmType
{
	DragEnd,
	Click,
	RightClick,
	DoubleClick,
}

sealed record DragStartActEvt(Pt PtStart) : IActEvt
{
	public override string ToString() => $"DragStart({PtStart})";
}
sealed record ConfirmActEvt(ConfirmType Type, Pt PtStart, Pt PtEnd) : IActEvt
{
	public override string ToString() => $"Confirm({Type}, ({PtStart}), ({PtEnd}))";
}
sealed record KeyDownActEvt(Keys Key) : IActEvt
{
	public override string ToString() => $"KeyDown({Key})";
}