namespace LinqVec.Tools.Acts.Structs;

public enum ActGfxState
{
	Hover,
	DragStart,
	Confirm
}

public sealed record ActGfxEvt(string Id, ActGfxState State);
