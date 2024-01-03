namespace RenderLib.Structs;

public enum DashStyleDef
{
	Solid,
	Dash,
	Dot,
	DashDot,
	DashDotDot,
}

public sealed record PenDef(Color Color, float Width)
{
	public DashStyleDef DashStyle { get; init; } = DashStyleDef.Solid;
}