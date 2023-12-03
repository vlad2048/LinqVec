using LinqVec.Structs;
using LinqVec.Utils;
using System.Drawing.Drawing2D;
using PowBasics.CollectionsExt;

namespace LinqVec.Drawing;


public static class DrawingExt
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
}