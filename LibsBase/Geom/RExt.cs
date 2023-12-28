namespace Geom;

public static class RExt
{
	public static R Enlarge(this R r, float radius) => new(
		new Pt(r.Min.X - radius, r.Min.Y - radius),
		new Pt(r.Max.X + radius, r.Max.Y + radius)
	);

	public static R? Union(this IEnumerable<R> source)
	{
		var arr = source.ToArray();
		if (arr.Length == 0) return null;
		var minX = float.MaxValue;
		var minY = float.MaxValue;
		var maxX = float.MinValue;
		var maxY = float.MinValue;
		foreach (var r in arr)
		{
			if (r.Min.X < minX) minX = r.Min.X;
			if (r.Min.Y < minY) minY = r.Min.Y;
			if (r.Max.X > maxX) maxX = r.Max.X;
			if (r.Max.Y > maxY) maxY = r.Max.Y;
		}
		return new R(new Pt(minX, minY), new Pt(maxX, maxY));
	}
}