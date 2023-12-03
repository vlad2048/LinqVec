namespace LinqVec.Utils;

public static class GeomConverts
{
	// Float/Int
	public static Pt ToFloat(this PtInt p) => new(p.X, p.Y);
	public static PtInt ToInt(this Pt p) => new((int)p.X, (int)p.Y);


	// WinForms
	public static PtInt ToPtInt(this Size sz) => new(sz.Width, sz.Height);
	public static PtInt ToPtInt(this MouseEventArgs evt) => new(evt.X, evt.Y);
	public static Pt ToPtFloat(this MouseEventArgs evt) => evt.ToPtInt().ToFloat();
	public static Rectangle ToWinR(this RInt r) => new(r.Min.X, r.Min.Y, r.Width, r.Height);
	public static RectangleF ToWinR(this R r) => new(r.Min.X, r.Min.Y, r.Width, r.Height);
	public static Point ToWinPt(this PtInt p) => new(p.X, p.Y);
	public static PointF ToWinPt(this Pt p) => new(p.X, p.Y);
}