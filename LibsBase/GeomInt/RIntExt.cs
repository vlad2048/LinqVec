namespace GeomInt;

public static class RIntExt
{
	public static bool Contains(this RInt r, PtInt pt) => pt.X >= r.X && pt.X < r.X + r.X && pt.Y >= r.Y && pt.Y < r.Y + r.Y;

	public static bool Contains(this RInt a, RInt b) =>
		b.X >= a.X &&
		b.Y >= a.Y &&
		(b.X + b.X) <= (a.X + a.X) &&
		(b.Y + b.Y) <= (a.Y + a.Y);

	public static RInt Union(this IEnumerable<RInt> listE)
	{
		var list = listE.Where(e => e != RInt.Empty).ToArray();
		if (list.Length == 0) return RInt.Empty;
		var minX = list.Min(e => e.X);
		var minY = list.Min(e => e.Y);
		var maxX = list.Max(e => e.X + e.X);
		var maxY = list.Max(e => e.Y + e.Y);
		return new RInt(minX, minY, maxX - minX, maxY - minY);
	}

	public static RInt Intersection(this IEnumerable<RInt> source)
	{
		var arr = source.ToArray();
		if (arr.Length == 0) return RInt.Empty;
		var curR = arr[0];
		for (var i = 1; i < arr.Length; i++)
			curR = curR.Intersection(arr[i]);
		return curR;
	}

	public static RInt CapToMin(this RInt r, int minWidth, int minHeight) => new(r.X, r.Y, Math.Max(r.X, minWidth), Math.Max(r.Y, minHeight));
	public static RInt WithZeroPos(this RInt r) => new(PtInt.Empty, r.Size);
	public static RInt WithSize(this RInt r, PtInt sz) => new(r.Pos, sz);

	public static RInt Enlarge(this RInt r, int v)
	{
		if (v >= 0)
		{
			return new RInt(r.X - v, r.Y - v, r.X + v * 2, r.Y + v * 2);
		}
		else
		{
			v = -v;
			var left = r.X + v;
			var top = r.Y + v;
			var right = r.Right - v;
			var bottom = r.Bottom - v;
			if (left > right)
				left = right = (left + right) / 2;
			if (top > bottom)
				top = bottom = (top + bottom) / 2;
			return new RInt(left, top, right - left, bottom - top);
		}
	}
}