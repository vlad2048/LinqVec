using System.Drawing.Drawing2D;
using System.Numerics;
using System.Reactive.Disposables;
using Geom;
using LinqVec.Drawing;

namespace LinqVec.Structs;

public sealed record Gfx(
	Graphics Graphics,
	Pt ClientSz,
	Transform Transform,
	Res Res
);

public readonly record struct GPen(
	uint ColorValue,
	float Thickness,
	bool IsPx,
	DashStyle DashStyle = DashStyle.Solid
)
{
	public Color Color => MkCol(ColorValue);
}

public static class GfxExt
{
	public static IDisposable UsePixels(this Gfx gfx)
	{
		var transform = gfx.Graphics.Transform;
		gfx.Graphics.Transform = new Matrix(Matrix3x2.Identity);
		return Disposable.Create(() => gfx.Graphics.Transform = transform);
	}

	public static R ClientR(this Gfx gfx) => new(new Pt(0, 0), new Pt(gfx.ClientSz.X, gfx.ClientSz.Y));

	public static Brush Brush(this Gfx gfx, Color color) => gfx.Res.Brush(color);
	public static Pen Pen(this Gfx gfx, GPen pen) => pen.IsPx switch
	{
		false => gfx.Res.Pen(pen.Color, pen.Thickness, pen.DashStyle),
		true => gfx.Res.Pen(pen.Color, pen.Thickness, pen.DashStyle, gfx.Transform.Zoom),
	};
}