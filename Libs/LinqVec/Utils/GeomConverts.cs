using Geom;

namespace LinqVec.Utils;

public static class GeomConverts
{
	// WinForms
	public static Pt ToPt(this MouseEventArgs evt) => new(evt.X, evt.Y);
	public static Pt ToPt(this Size sz) => new(sz.Width, sz.Height);
	public static RectangleF ToWinR(this R r) => new(r.Min.X, r.Min.Y, r.Width, r.Height);
	public static PointF ToWinPt(this Pt p) => new(p.X, p.Y);
}