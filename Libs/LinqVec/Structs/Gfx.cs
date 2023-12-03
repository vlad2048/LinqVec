using LinqVec.Drawing;

namespace LinqVec.Structs;

public sealed record Gfx(
	Graphics Graphics,
	PtInt ClientSz,
	Transform Transform,
	Res Res
);

public readonly record struct GPen(
	uint ColorValue,
	float Thickness,
	bool IsPx
)
{
	public Color Color => MkCol(ColorValue);
}

public static class GfxExt
{
	public static R ClientR(this Gfx gfx) => new(new Pt(0, 0), new Pt(gfx.ClientSz.X, gfx.ClientSz.Y));

	public static Brush Brush(this Gfx gfx, Color color) => gfx.Res.Brush(color);
	public static Pen Pen(this Gfx gfx, GPen pen) => pen.IsPx switch
	{
		false => gfx.Res.Pen(pen.Color, pen.Thickness),
		true => gfx.Res.Pen(pen.Color, pen.Thickness, gfx.Transform.Zoom),
	};
}