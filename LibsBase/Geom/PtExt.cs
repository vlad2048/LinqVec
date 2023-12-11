namespace Geom;

public static class PtExt
{
	public static R GetBBox(this IEnumerable<Pt> source)
	{
		var arr = source.ToArray();
		var xMin = arr.Min(e => e.X);
		var yMin = arr.Min(e => e.Y);
		var xMax = arr.Max(e => e.X);
		var yMax = arr.Max(e => e.Y);
		return new R(new Pt(xMin, yMin), new Pt(xMax, yMax));
	}
}