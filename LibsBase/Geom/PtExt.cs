using System.Numerics;

namespace Geom;

public static class PtExt
{
	public static RGen<T> GetBBox<T>(this IEnumerable<PtGen<T>> source) where T : struct, INumber<T>
	{
		var arr = source.ToArray();
		var xMin = arr.Min(e => e.X);
		var yMin = arr.Min(e => e.Y);
		var xMax = arr.Max(e => e.X);
		var yMax = arr.Max(e => e.Y);
		return new RGen<T>(new PtGen<T>(xMin, yMin), new PtGen<T>(xMax, yMax));
	}
}