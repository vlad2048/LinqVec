using System.Drawing.Drawing2D;
using LinqVec.Drawing;
using LinqVec.Structs;
using LinqVec.Tools.Curve_.Events;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using PowMaybe;
using PowRxVar;

namespace LinqVec.Tools.Curve_.Model;

static class CurveModelPainter
{
    public static void Draw(
        Gfx gfx,
        CurveModel model,
        IRoVar<CurveState> curveState,
        IRoVar<Maybe<Pt>> mousePos
    )
    {
        var pts = model.Points;
        var cnt = pts.Count;
        if (cnt == 0) return;

        // Bezier curve
        // ============
        if (cnt > 1)
        {
            var winPts = pts
                .SelectMany(p => new[]
                {
                    p.HLeft,
                    p.P,
                    p.HRight
                })
                .Skip(1)
                .SkipLast(1)
                .SelectToList(GeomConverts.ToWinPt);
            if (curveState.V == CurveState.Move && mousePos.V.IsSome(out var mp))
            {
                winPts.Add(pts.Last().HRight.ToWinPt());
                winPts.Add(mp.ToWinPt());
                winPts.Add(mp.ToWinPt());
            }
            using var gfxPath = new GraphicsPath();
            gfxPath.AddBeziers(winPts.ToArray());
            gfx.Graphics.DrawPath(gfx.Pen(C.PenCurve), gfxPath);
        }

        // Finished control points
        // =======================
        for (var i = 0; i < cnt - 1; i++)
            gfx.DrawMarker(pts[i].P, MarkerType.CurvePoint);

        // Control point in progress
        // =========================
        gfx.DrawMarker(pts[cnt - 1].P, MarkerType.CurvePointProgress);
        if (pts[cnt - 1].HasHandles)
        {
            gfx.DrawMarker(pts[cnt - 1].HLeft, MarkerType.CurveHandle);
            gfx.DrawMarker(pts[cnt - 1].HRight, MarkerType.CurveHandle);
            gfx.Line(pts[cnt - 1].HLeft, pts[cnt - 1].HRight, C.Markers.PointPen);
        }

        // Draw the segment that would be drawn if the user chooses this next point
        // ========================================================================

    }
}