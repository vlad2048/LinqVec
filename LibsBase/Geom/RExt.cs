namespace Geom;

public static class RExt
{
	public static R Enlarge(this R r, float radius) => new(
		new Pt(r.Min.X - radius, r.Min.Y - radius),
		new Pt(r.Max.X + radius, r.Max.Y + radius)
	);
}