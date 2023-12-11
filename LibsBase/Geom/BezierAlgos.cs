using Pt = Geom.PtGen<float>;

namespace Geom;

public static class BezierAlgos
{
	private const int SubDivs = 100;

	public static double DistanceToPoint(this Pt[] bezier, Pt p) => Sample(bezier).Min(e => (p - e).Length);

	private static Pt[] Sample(Pt[] pts)
	{
		var ptsCnt = pts.Length;
		if (ptsCnt % 3 != 1) throw new ArgumentException();
		var cnt = ptsCnt / 3;
		return
			Enumerable.Range(0, cnt)
				.SelectMany(i => DrawSeg(pts.Skip(i * 3).Take(4)))
				.ToArray();
	}

	private static Pt[] DrawSeg(IEnumerable<Pt> pts)
	{
		var arr = pts.ToArray();
		if (arr.Length != 4) throw new ArgumentException();
		return DrawSeg(arr[0], arr[1], arr[2], arr[3]);
	}
	private static Pt[] DrawSeg(Pt p1, Pt p2, Pt p3, Pt p4) =>
		Enumerable.Range(0, SubDivs + 1)
			.Select(e => e * 1f / SubDivs)
			.Select(t => p1 * (1 - t).Pow(3) + p2 * 3 * (1 - t).Pow(2) * t + p3 * 3 * (1 - t) * t.Pow(2) + p4 * t.Pow(3))
			.ToArray();

	private static float Pow(this float v, int p) => MathF.Pow(v, p);
}