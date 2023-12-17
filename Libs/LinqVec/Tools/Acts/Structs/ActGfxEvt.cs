namespace LinqVec.Tools.Acts.Structs;

public enum ActGfxState
{
	Hover,
	DragStart,
	Confirm
}

public sealed record ActGfxEvt(
	string ActSetId,
	string Id,
	ActGfxState State
)
{
	public override string ToString() => $"[{ActSetId}].[{Id}].{State}";
}
