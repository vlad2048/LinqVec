using LinqVec.Drawing;
using LinqVec.Structs;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Drawing;

namespace VectorEditor.Tools.Curve_.Utils;

static class CurveModelPainter
{
	public static void Draw(
		Gfx gfx,
		CurveModel model
	)
	{
		var pts = model.Pts;
		var cnt = pts.Length;
		if (cnt == 0) return;

		// Bezier curve
		// ============
		gfx.DrawBezier(
			C.PenCurve,
			pts
				.SelectMany(p => new[]
				{
					p.HLeft,
					p.P,
					p.HRight
				})
				.Skip(1)
				.SkipLast(1)
		);

		// Finished control points
		// =======================
		for (var i = 0; i < cnt - 1; i++)
			gfx.DrawCurvePointMarkers(pts[i], false);

		// Control point in progress
		// =========================
		gfx.DrawCurvePointMarkers(pts[cnt - 1], true);
	}
}