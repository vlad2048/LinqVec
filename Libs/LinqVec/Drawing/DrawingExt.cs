using LinqVec.Structs;
using LinqVec.Utils;
using PowMaybe;

namespace LinqVec.Drawing;

enum MarkerType
{
    CurvePoint,
    CurvePointProgress,
    CurveHandle
}

static class DrawingExt
{
    public static void DrawR(this Gfx gfx, R r, GPen pen) => gfx.Graphics.DrawRectangle(gfx.Pen(pen), r.ToWinR());
	public static void FillR(this Gfx gfx, R r, Color brush) => gfx.Graphics.FillRectangle(gfx.Brush(brush), r.ToWinR());
	public static void DrawCircle(this Gfx gfx, R r, GPen pen) => gfx.Graphics.DrawEllipse(gfx.Pen(pen), r.ToWinR());
	public static void FillCircle(this Gfx gfx, R r, Color brush) => gfx.Graphics.FillEllipse(gfx.Brush(brush), r.ToWinR());
    public static void Line(this Gfx gfx, Pt a, Pt b, GPen pen) => gfx.Graphics.DrawLine(gfx.Pen(pen), a.ToWinPt(), b.ToWinPt());

    public static void DrawMarker(this Gfx gfx, Maybe<Pt> mayP, MarkerType type)
    {
	    if (mayP.IsNone(out var p)) return;
        gfx.DrawMarker(p, type);
    }

    public static void DrawMarker(this Gfx gfx, Pt p, MarkerType type)
    {
	    var r = R.FromCenter(p, C.Markers.Radius / gfx.Transform.Zoom);
	    switch (type)
	    {
            case MarkerType.CurvePoint:
	            gfx.DrawR(r, C.Markers.PointPen);
	            break;
            case MarkerType.CurvePointProgress:
                gfx.FillR(r, C.Markers.PointBrush);
                gfx.DrawR(r, C.Markers.PointPen);
	            break;
            case MarkerType.CurveHandle:
                gfx.FillCircle(R.FromCenter(p, (C.Markers.Radius + 1) / gfx.Transform.Zoom), C.Markers.PointBrush);
	            break;
	    }
	}
}