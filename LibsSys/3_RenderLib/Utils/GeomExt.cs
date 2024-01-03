namespace RenderLib.Utils;

public static class GeomExt
{
	public static Point ToDrawPt(this Pt r) => new(r.X, r.Y);
	public static PointF ToDrawPtF(this Pt r) => new(r.X, r.Y);
	public static Rectangle ToDrawRect(this R r) => new(r.X, r.Y, r.Width, r.Height);
	public static RectangleF ToDrawRectF(this R r) => new(r.X, r.Y, r.Width, r.Height);
}