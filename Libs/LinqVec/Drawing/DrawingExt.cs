using LinqVec.Structs;
using LinqVec.Utils;
using PowMaybe;
using System.Drawing.Drawing2D;
using LinqVec.Tools.Curve_.Model;
using PowBasics.CollectionsExt;

namespace LinqVec.Drawing;

enum MarkerType
{
    CurvePoint,
    CurvePointProgress,
    CurveHandle
}

static class DrawingExt
{
	public static void DrawBezier(this Gfx gfx, GPen pen, IEnumerable<Pt> pts)
	{
		var winPts = pts.SelectToArray(e => e.ToWinPt());
		if (winPts.Length < 4) return;
		using var path = new GraphicsPath();
		path.AddBeziers(winPts);
		gfx.Graphics.DrawPath(gfx.Pen(pen), path);
	}

    public static void DrawR(this Gfx gfx, R r, GPen pen) => gfx.Graphics.DrawRectangle(gfx.Pen(pen), r.ToWinR());
	public static void FillR(this Gfx gfx, R r, Color brush) => gfx.Graphics.FillRectangle(gfx.Brush(brush), r.ToWinR());
	public static void DrawCircle(this Gfx gfx, R r, GPen pen) => gfx.Graphics.DrawEllipse(gfx.Pen(pen), r.ToWinR());
	public static void FillCircle(this Gfx gfx, R r, Color brush) => gfx.Graphics.FillEllipse(gfx.Brush(brush), r.ToWinR());
    public static void Line(this Gfx gfx, Pt a, Pt b, GPen pen) => gfx.Graphics.DrawLine(gfx.Pen(pen), a.ToWinPt(), b.ToWinPt());

    public static void DrawMouseMarker(this Gfx gfx, Maybe<Pt> mayPt)
    {
	    if (mayPt.IsNone(out var pt)) return;
		gfx.DrawMarker(pt, MarkerType.CurvePointProgress, C.Markers.PointPen, C.Markers.PointBrush);
    }

    public static void DrawCurvePointMarkers(this Gfx gfx, CurvePt pt, bool inProgress)
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

	/*public static void DrawMarker(this Gfx gfx, Maybe<Pt> mayP, MarkerType type, bool done)
    {
	    if (mayP.IsNone(out var p)) return;
        gfx.DrawMarker(p, type, done);
    }

    public static void DrawMarker(this Gfx gfx, Pt p, MarkerType type, bool done)
    {
	    var r = R.FromCenter(p, C.Markers.Radius / gfx.Transform.Zoom);
	    var pen = done ? C.Markers.PointPenDone : C.Markers.PointPen;
	    var brush = done ? C.Markers.PointBrushDone : C.Markers.PointBrush;
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
	}*/
}