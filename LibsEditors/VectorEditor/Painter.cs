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
	private static readonly GPen PenCurve = new(0x5B94F1, 1, true);
	private static readonly GPen PenCurveProgress = new(0xFF9B9B, 1, true);
	private static readonly GPen PenCurveSoft = new(0xD4D5D6, 1, true);

	public static void DrawCurve(Gfx gfx, Curve curve, bool isAddingPoint)
	{
		if (curve.Closed)
		{
			gfx.DrawBezier(PenCurve, curve.Pts.GetClosedPoints());
			DrawCurvePoints(gfx, curve.Pts, PenCurve);
		}
		else
		{
			gfx.DrawBezier(
				PenCurve,
				curve.Pts.SkipLast(1).GetOpenPoints()
			);
			DrawCurvePoints(gfx, curve.Pts.SkipLast(1), PenCurve);
			DrawCurveHandles(gfx, curve.Pts.SkipLast(1), PenCurveSoft);

			var lastSegPen = isAddingPoint ? PenCurveProgress : PenCurve;
			var lastSegPenSoft = isAddingPoint ? PenCurveProgress : PenCurveSoft;
			gfx.DrawBezier(
				lastSegPen,
				curve.Pts.TakeLast(2).GetOpenPoints()
			);
			DrawCurvePoints(gfx, curve.Pts.TakeLast(1), lastSegPen);
			DrawCurveHandles(gfx, curve.Pts.TakeLast(1), lastSegPenSoft);
		}
	}

	public static void DrawHoverSeg(Gfx gfx, Curve curve, Pt mouse)
	{
		if (curve.Pts.Length > 0)
			gfx.DrawBezier(PenCurveProgress, new[] { curve.Pts[^1], CurvePt.Make(null, mouse) }.GetOpenPoints());

		var r = R.FromCenter(mouse, CurveMarkerRadius / gfx.Transform.Zoom);
		gfx.DrawR(r, PenCurveProgress);
	}


	// @formatter:off
	private static Pt[] GetOpenPoints(this IEnumerable<CurvePt> pts) => pts.FlattenPoints().Skip(1).SkipLast(1).ToArray();
	private static Pt[] GetClosedPoints(this IEnumerable<CurvePt> pts) { var flatPts = pts.FlattenPoints(); return flatPts.Skip(1).Concat(flatPts.Take(2)).ToArray(); }
	// @formatter:on

	private static Pt[] FlattenPoints(this IEnumerable<CurvePt> pts) => pts.SelectMany(e => new[] { e.HLeft, e.P, e.HRight }).ToArray();


	private static void DrawCurvePoints(Gfx gfx, IEnumerable<CurvePt> pts, GPen pen)
	{
		void DrawPoint(Pt p)
		{
			var r = R.FromCenter(p, CurveMarkerRadius / gfx.Transform.Zoom);
			gfx.DrawR(r, pen);
		}

		foreach (var pt in pts)
			DrawPoint(pt.P);
	}

	private static void DrawCurveHandles(Gfx gfx, IEnumerable<CurvePt> pts, GPen pen)
	{
		void DrawHandles(CurvePt pt)
		{
			void DrawHandle(Pt p)
			{
				var r = R.FromCenter(p, (CurveMarkerRadius + 1) / gfx.Transform.Zoom);
				gfx.DrawCircle(r, pen);
			}

			DrawHandle(pt.HLeft);
			DrawHandle(pt.HRight);
			gfx.Line(pt.HLeft, pt.HRight, pen);
		}
		foreach (var pt in pts)
			DrawHandles(pt);
	}




	/*
	private const int CurveMarkerRadius = 6;
	private static readonly GPen CurvePen = new(0x5B94F1, 1, true);
	private static readonly GPen CurvePenPrevious = new(0xD4D5D6, 1, true);
	private static readonly GPen CurvePenProgress = new(0xFF9B9B, 1, true);

	public static void PaintCurve(
		Gfx gfx,
		Curve model,
		bool isAddingPoint

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


		pts.ForEach(pt => DrawPoint(gfx, pt.P));

		if (isAddingPoint)
		{
			gfx.DrawBezier(
				CurvePenProgress,
				pts
					.Skip(pts.Length - 2)
					.SelectMany(p => new[] {
						p.HLeft,
						p.P,
						p.HRight
					})
					.Skip(1)
					.SkipLast(1)
			);
		}

		for (var i = 0; i < pts.Length - 1; i++)
			DrawHandles(gfx, pts[i], false);
		if (pts.Length > 0)
			DrawHandles(gfx, pts[^1], true);
	}

	public static void DrawHoverSegment(Gfx gfx, Curve curve, Pt mousePos)
	{
		DrawPoint(gfx, mousePos);
		var pts = curve.Pts;
		if (pts.Length == 0) return;

		gfx.DrawBezier(
			CurvePenProgress,
			pts
				.Skip(pts.Length - 2)
				.Append(CurvePt.Make(null, mousePos))
				.SelectMany(p => new[] {
					p.HLeft,
					p.P,
					p.HRight
				})
				.Skip(1)
				.SkipLast(1)
		);
	}


	public static void DrawPoint(Gfx gfx, Pt p)
	{
		var r = R.FromCenter(p, CurveMarkerRadius / gfx.Transform.Zoom);
		gfx.DrawR(r, CurvePen);
	}

	private static void DrawHandles(Gfx gfx, CurvePt pt, bool isCurrent)
	{
		var pen = isCurrent ? CurvePen : CurvePenPrevious;
		DrawHandle(gfx, pt.HLeft, isCurrent);
		DrawHandle(gfx, pt.HRight, isCurrent);
		gfx.Line(pt.HLeft, pt.HRight, pen);
	}

	private static void DrawHandle(Gfx gfx, Pt p, bool isCurrent)
	{
		var pen = isCurrent ? CurvePen : CurvePenPrevious;
		var r = R.FromCenter(p, (CurveMarkerRadius + 1) / gfx.Transform.Zoom);
		gfx.DrawCircle(r, pen);
	}
	*/
}