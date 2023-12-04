using LinqVec.Structs;
using PowMaybe;
using LinqVec.Drawing;
using VectorEditor.Model.Structs;

namespace VectorEditor.Tools.Curve_.Drawing;

enum MarkerType
{
	CurvePoint,
	CurvePointProgress,
	CurveHandle
}

static class CurveDrawExt
{
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

	public static void DrawMarker(this Gfx gfx, Pt p, MarkerType type, GPen pen, Color brush)
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
}