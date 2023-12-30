using Geom;
using LinqVec.Drawing;
using LinqVec.Structs;
using PowBasics.CollectionsExt;
using System.Drawing.Drawing2D;
using VectorEditor._Model;
using VectorEditor._Model.Structs;

namespace VectorEditor;


enum CurveGfxState
{
	None,
	Edit,
	AddPoint,
	DragHandle,
}


static class Painter
{
	// **********
	// * Select *
	// **********
	private static readonly GPen SelectPen = new(0x000000, 1, false, DashStyle.Dash);

	public static void PaintSelectRectangle(
		Gfx gfx,
		R? bboxOpt
	)
	{
		if (!bboxOpt.HasValue) return;
		var bbox = bboxOpt.Value;
		var r = bbox.ToPixel(gfx.Transform).Enlarge(5);
		using (gfx.UsePixels())
			gfx.DrawR(r, SelectPen);
	}



	// *********
	// * Curve *
	// *********
	private const int CurveMarkerRadius = 6;
	private static readonly GPen CurvePen = new(0x5B94F1, 1, true);
	private static readonly GPen CurvePenProgress = new(0xFF9B9B, 1, true);

	public static void PaintCurve(
		Gfx gfx,
		Curve model,
		CurveGfxState state

	)
	{
		var pts = model.Pts;
		var cnt = pts.Length;
		if (cnt == 0) return;

		// Bezier curve
		// ============
		gfx.DrawBezier(
			CurvePen,
			model.GetDrawPoints()
		);


		if (state is not CurveGfxState.None)
			pts.ForEach(pt => DrawPoint(gfx, pt.P));

		if (state is CurveGfxState.AddPoint or CurveGfxState.DragHandle)
			gfx.DrawBezier(
				CurvePenProgress,
				pts
					.Skip(pts.Length - 2)
					.SelectMany(p => new[]
					{
						p.HLeft,
						p.P,
						p.HRight
					})
					.Skip(1)
					.SkipLast(1)
			);

		if (state is CurveGfxState.AddPoint)
			if (pts.Length > 1)
				DrawHandles(gfx, pts[^2]);

		if (state is CurveGfxState.DragHandle)
			if (pts.Length > 0)
				DrawHandles(gfx, pts[^1]);
	}


	private static void DrawPoint(Gfx gfx, Pt p)
	{
		var r = R.FromCenter(p, CurveMarkerRadius / gfx.Transform.Zoom);
		gfx.DrawR(r, CurvePen);
	}

	private static void DrawHandles(Gfx gfx, CurvePt pt)
	{
		DrawHandle(gfx, pt.HLeft);
		DrawHandle(gfx, pt.HRight);
		gfx.Line(pt.HLeft, pt.HRight, CurvePen);
	}

	private static void DrawHandle(Gfx gfx, Pt p)
	{
		var r = R.FromCenter(p, (CurveMarkerRadius + 1) / gfx.Transform.Zoom);
		gfx.DrawCircle(r, CurvePen);
	}
}