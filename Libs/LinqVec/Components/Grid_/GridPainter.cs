using Geom;
using LinqVec.Drawing;
using LinqVec.Structs;
using LinqVec.Utils;
using PowBasics.CollectionsExt;

namespace LinqVec.Components.Grid_;

static class GridPainter
{
	public static void DrawAndSetTransform(Gfx gfx)
	{
		// Background + Transform
		// ======================
		var clientR = gfx.ClientR();
		gfx.FillR(clientR, C.GridGfx.BackColor);
		gfx.Graphics.Transform = gfx.Transform.Matrix;

		// Grid
		// ====
		var extent = C.Grid.TickCount * C.Grid.TickSize;
		for (var i = -C.Grid.TickCount; i <= C.Grid.TickCount; i++)
		{
			if (i == 0) continue;
			var isLarge = i % C.GridGfx.LargeTickMultiple == 0;
			var pen = isLarge ? C.GridGfx.PenGridLarge : C.GridGfx.PenGridSmall;
			var t = i * C.Grid.TickSize;
			var sz = extent;
			gfx.Line(new Pt(t, -sz), new Pt(t, sz), pen);
			gfx.Line(new Pt(-sz, t), new Pt(sz, t), pen);
		}

		var axeSz = extent + C.Grid.TickSize;
		var axePen = C.GridGfx.PenAxe;
		gfx.LineArrowEnd(new Pt(0, -axeSz), new Pt(0, axeSz), axePen);
		gfx.LineArrowEnd(new Pt(-axeSz, 0), new Pt(axeSz, 0), axePen);

		for (var i = -C.Grid.TickCount; i <= C.Grid.TickCount; i++)
		{
			if (i == 0) continue;
			var isLarge = i % C.GridGfx.LargeTickMultiple == 0;
			var pen = isLarge ? C.GridGfx.PenAxeTickLarge : C.GridGfx.PenAxeTickSmall;
			var t = i * C.Grid.TickSize;
			var sz = (isLarge ? C.GridGfx.AxeTickSizeLargePx : C.GridGfx.AxeTickSizeSmallPx) / gfx.Transform.Zoom;
			gfx.Line(new Pt(t, -sz), new Pt(t, sz), pen);
			gfx.Line(new Pt(-sz, t), new Pt(sz, t), pen);
		}
	}


	private static void LineArrowEnd(this Gfx gfx, Pt a, Pt b, GPen pen)
	{
		var u = (b - a).Norm();
		var v = new Pt(-u.Y, u.X);
		var halfBase = C.GridGfx.ArrowHalfBase / gfx.Transform.Zoom;
		var length = C.GridGfx.ArrowLength / gfx.Transform.Zoom;
		b -= u * length;
		var b0 = b + v * halfBase;
		var b1 = b - v * halfBase;
		var e = b + u * length;
		var brush = gfx.Res.Brush(Color.Black);
		var pts = new[] { b0, b1, e }.SelectToArray(f => f.ToWinPt());
		gfx.Line(a, b, pen);
		gfx.Graphics.FillPolygon(brush, pts, System.Drawing.Drawing2D.FillMode.Winding);
	}
}