/*
using PowBasics.Geom;
using WinAPI.NetCoreEx.Geometry;

namespace WinAPI.Utils.Exts;

public static class GeomExt
{
	public static Size ToSize(this Sz s) => new(s.Width, s.Height);
	public static Sz ToSz(this Size s) => new(s.Width, s.Height);
	public static System.Drawing.Size ToDrawSz(this Size s) => new(s.Width, s.Height);

	public static Point ToPoint(this Pt p) => new(p.X, p.Y);
	public static Pt ToPt(this Point p) => new(p.X, p.Y);
	public static System.Drawing.Point ToDrawPt(this Pt p) => new(p.X, p.Y);
	public static System.Drawing.Point ToDrawPt(this Point p) => new(p.X, p.Y);

	public static Rectangle ToRect(this R r) => new(r.X, r.Y, r.Right, r.Bottom);
	public static R ToR(this Rectangle r) => new(r.Left, r.Top, r.Width, r.Height);
	public static System.Drawing.Rectangle ToDrawRect(this R r) => new(r.X, r.Y, r.Width, r.Height);
	public static System.Drawing.RectangleF ToDrawRectF(this R r) => new(r.X, r.Y, r.Width, r.Height);


	public static Sz Cap(this Sz a, Sz b) => new(
		Math.Min(a.Width, b.Width),
		Math.Min(a.Height, b.Height)
	);
}
*/