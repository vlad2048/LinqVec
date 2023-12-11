using LinqVec.Drawing;
using LinqVec.Structs;
using PowBasics.CollectionsExt;
using VectorEditor.Model;
using VectorEditor.Model.Structs;

namespace VectorEditor.Tools.Curve_;

enum CurveGfxState
{
    None,
    Edit,
    AddPoint,
    DragHandle,
}

static class CurvePainter
{
	private const int MarkerRadius = 6;
	private static readonly GPen Pen = new(0x5B94F1, 1, true);
	private static readonly GPen PenProgress = new(0xFF9B9B, 1, true);

	public static void Draw(
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
            Pen,
            model.GetDrawPoints()
        );


        if (state is not CurveGfxState.None)
            pts.ForEach(pt => DrawPoint(gfx, pt.P));

        if (state is CurveGfxState.AddPoint or CurveGfxState.DragHandle)
	        gfx.DrawBezier(
		        PenProgress,
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


			/*
			// Finished control points
			// =======================
			for (var i = 0; i < cnt - 1; i++)
				gfx.DrawCurvePointMarkers(pts[i], false);

			// Control point in progress
			// =========================
			gfx.DrawCurvePointMarkers(pts[cnt - 1], true);
			*/
	}


	private static void DrawPoint(Gfx gfx, Pt p)
	{
		var r = R.FromCenter(p, MarkerRadius / gfx.Transform.Zoom);
		gfx.DrawR(r, Pen);
	}

	private static void DrawHandles(Gfx gfx, CurvePt pt)
	{
		DrawHandle(gfx, pt.HLeft);
		DrawHandle(gfx, pt.HRight);
		gfx.Line(pt.HLeft, pt.HRight, Pen);
	}

	private static void DrawHandle(Gfx gfx, Pt p)
	{
		var r = R.FromCenter(p, (MarkerRadius + 1) / gfx.Transform.Zoom);
		gfx.DrawCircle(r, Pen);
	}



	/*
    private static void DrawCurvePointMarkers(this Gfx gfx, CurvePt pt, bool inProgress)
    {
	    var pen = inProgress ? C.Markers.PointPen : C.Markers.PointPenDone;
	    var brush = inProgress ? C.Markers.PointBrush : C.Markers.PointBrushDone;

	    gfx.DrawMarker(pt.P, inProgress ? MarkerType.CurvePointProgress : MarkerType.CurvePoint, pen, brush);
	    if (pt.HasHandles)
	    {
		    gfx.DrawMarker(pt.HLeft, MarkerType.CurveHandle, pen, brush);
		    gfx.DrawMarker(pt.HRight, MarkerType.CurveHandle, pen, brush);
		    gfx.Line(pt.HLeft, pt.HRight, pen);
	    }
    }

    private static void DrawMarker(this Gfx gfx, Pt p, MarkerType type, GPen pen, Color brush)
    {
	    var r = R.FromCenter(p, C.Markers.Radius / gfx.Transform.Zoom);
	    switch (type)
	    {
		    case MarkerType.CurvePoint:
			    gfx.DrawR(r, pen);
			    break;
		    case MarkerType.CurvePointProgress:
			    gfx.FillR(r, brush);
			    gfx.DrawR(r, pen);
			    break;
		    case MarkerType.CurveHandle:
			    gfx.FillCircle(R.FromCenter(p, (C.Markers.Radius + 1) / gfx.Transform.Zoom), brush);
			    break;
	    }
    }
    */
}