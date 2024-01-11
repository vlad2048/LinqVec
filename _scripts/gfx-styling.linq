<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\LibsBase\Geom\bin\Debug\net8.0\Geom.dll</Reference>
  <Reference>C:\dev\big\LinqVec\Libs\LinqVec\bin\Debug\net8.0-windows\LinqVec.dll</Reference>
  <Namespace>Geom</Namespace>
  <Namespace>LinqVec.Components.Grid_</Namespace>
  <Namespace>LinqVec.Drawing</Namespace>
  <Namespace>LinqVec.Structs</Namespace>
  <Namespace>LinqVec.Utils</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Drawing2D</Namespace>
</Query>

const int BmpSize = 200;

void Main()
{
	var P0 = CurvePt.Make(new Pt(2, 2), new Pt(3, 1));
	var P1 = CurvePt.Make(new Pt(5, 2), new Pt(5, 3));
	var P2 = CurvePt.Make(new Pt(3, 5), new Pt(2, 5));
	var Pdrag = CurvePt.Make(new Pt(1, 4), new Pt(1, 2));
	var allPts = new CurvePt[] { P0, P1, P2 };
	var curveClosed = new Curve(allPts, true);

	for (var i = 0; i < allPts.Length; i++)
	{
		var pts = allPts.Take(i).ToArray();
		var curve = new Curve(pts, false);
		
		Util.Pivot(new
		{
			None = Show(gfx => Painter.DrawCurve(gfx, curve, false)),
			Hover = Show(gfx =>
			{
				Painter.DrawCurve(gfx, curve, false);
				Painter.DrawHoverSeg(gfx, curve, new Pt(1, 4));
				Painter.DrawMouse(gfx, new Pt(1, 4));
			}),
			Drag = Show(gfx =>
			{
				var curveDrag = new Curve([.. pts, Pdrag], false);
				Painter.DrawCurve(gfx, curveDrag, true);
				Painter.DrawMouse(gfx, Pdrag.HRight);
			})
		}).Dump($"{i} points");
	}

	Show(gfx =>
	{
		Painter.DrawCurve(gfx, curveClosed, false);
	}).Dump("Closed");
}


static Bitmap Show(Action<Gfx> action)
{
	var (gfx, bmp) = DrawUtils.Prep(BmpSize);
	action(gfx);
	return bmp;
}


public record CurvePt(Pt P, Pt HLeft, Pt HRight)
{
	public static CurvePt Make(Pt? startPt, Pt endPt) => startPt switch
	{
		null => new CurvePt(endPt, endPt, endPt),
		not null => new CurvePt(startPt.Value, startPt.Value - (endPt - startPt.Value), endPt)
	};
}
public record Curve(CurvePt[] Pts, bool Closed);


static class Painter
{
	private const int CurveMarkerRadius = 6;
	private static readonly GPen PenCurve = new(0x5B94F1, 1, true);
	private static readonly GPen PenCurveProgress = new(0xFF9B9B, 1, true);

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

			var lastSegPen = isAddingPoint ? PenCurveProgress : PenCurve;
			gfx.DrawBezier(
				lastSegPen,
				curve.Pts.TakeLast(2).GetOpenPoints()
			);
			DrawCurvePoints(gfx, curve.Pts.TakeLast(1), lastSegPen);
			if (isAddingPoint && curve.Pts.Length > 0)
				DrawCurveHandles(gfx, curve.Pts[^1]);
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
		void DrawCurvePoint(Pt p)
		{
			var r = R.FromCenter(p, CurveMarkerRadius / gfx.Transform.Zoom);
			gfx.DrawR(r, pen);
		}

		foreach (var pt in pts)
			DrawCurvePoint(pt.P);
	}

	private static void DrawCurveHandles(Gfx gfx, CurvePt pt)
	{
		var pen = PenCurveProgress;
		void DrawHandle(Pt p)
		{
			var r = R.FromCenter(p, (CurveMarkerRadius + 1) / gfx.Transform.Zoom);
			gfx.DrawCircle(r, pen);
		}
		DrawHandle(pt.HLeft);
		DrawHandle(pt.HRight);
		gfx.Line(pt.HLeft, pt.HRight, pen);
	}
	
	
	

	private static readonly Bitmap mouseBmp = new(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath)!, "mouse.png"));
	public static void DrawMouse(Gfx gfx, Pt pos)
	{
		var g = gfx.Graphics;
		var tr = g.Transform;
		g.ResetTransform();
		var p = pos.ToPixel(gfx.Transform).ToWinPt();
		g.DrawImage(mouseBmp, p);
		g.Transform = tr;
	}
}




static class DrawUtils
{
	private static readonly GfxResources gfxResources = new();

	public static (Gfx, Bitmap) Prep(int bmpSize)
	{
		var bmp = new Bitmap(bmpSize, bmpSize);
		var graphics = Graphics.FromImage(bmp);
		var clientSz = new Pt(bmp.Width, bmp.Height);
		var transform = Transform.MakeInitial(clientSz);
		transform = new Transform(18, 7, new Pt(10, 10));
		var gfx = new Gfx(graphics, clientSz, transform, gfxResources);
		GridPainter.DrawAndSetTransform(gfx);
		return (gfx, bmp);
	}
}





