<Query Kind="Program">
  <Reference>C:\dev\play\LinqVec\Libs\Geom\bin\Debug\net8.0\Geom.dll</Reference>
  <Reference>C:\dev\play\LinqVec\Libs\LinqVec\bin\Debug\net8.0-windows\LinqVec.dll</Reference>
  <Namespace>LinqVec</Namespace>
  <Namespace>LinqVec.Structs</Namespace>
  <Namespace>LinqVec.Utils</Namespace>
  <Namespace>LinqVec.Utils.Drawing</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

global using Pt = Geom.PtGen<float>;
global using PtInt = Geom.PtGen<int>;
global using static LinqVec.Utils.CommonMakers;


void Main()
{
	DrawGrid(512);
}


static void DrawGrid(int sizePx)
{
	var clientSz = new PtInt(sizePx, sizePx);
	var res = new Res();
	var transform = Tr.MakeInitial(clientSz);
	var bmp = new Bitmap(sizePx, sizePx);
	var graphics = Graphics.FromImage(bmp);
	var gfx = new Gfx(graphics, clientSz, transform, res);
	GridPainterTweak.DrawAndSetTransform(gfx);
	bmp.Dump();
}

static class Tr
{
	private static readonly Transform Id = new(1, C.ZoomLevelOne, Pt.Zero);
	public static Transform MakeInitial(PtInt clientSz)
	{
		var szPix = Math.Min(clientSz.X, clientSz.Y) - C.GridGfx.InitPaddingPx * 2;
		if (szPix <= 1) return Id;
		var szSys = C.Grid.TickSize * C.Grid.TickCount * 2;
		var result = new Transform(
			szPix / szSys,
			C.ZoomLevelOne,
			new Pt(
				clientSz.X / 2f,
				clientSz.Y / 2f
			)
		);
		if (result == Id) throw new ArgumentException("This shouldn't return Id as we use Id to represent no value");
		return result;
	}
}

static class GridPainterTweak
{
	public static void DrawAndSetTransform(Gfx gfx)
	{
		// Background + Transform
		// ======================
		var clientR = gfx.ClientR();
		gfx.FillR(clientR, gfx.Res.Brush(C.GridGfx.BackColor));
		gfx.Graphics.Transform = gfx.Transform.Matrix;
		
		// Grid
		// ====
		var extent = C.Grid.TickCount * C.Grid.TickSize;
		for (var i = -C.Grid.TickCount; i <= C.Grid.TickCount; i++)
		{
			if (i == 0) continue;
			var isLarge = i % C.GridGfx.LargeTickMultiple == 0;
			var pen = gfx.Pen(isLarge ? C.GridGfx.PenGridLarge : C.GridGfx.PenGridSmall);
			var t = i * C.Grid.TickSize;
			var sz = extent;
			gfx.Line(new Pt(t, -sz), new Pt(t, sz), pen);
			gfx.Line(new Pt(-sz, t), new Pt(sz, t), pen);
		}
		
		var axeSz = extent + C.Grid.TickSize;
		var axePen = gfx.Pen(C.GridGfx.PenAxe);
		gfx.LineArrowEnd(new Pt(0, -axeSz), new Pt(0, axeSz), axePen);
		gfx.LineArrowEnd(new Pt(-axeSz, 0), new Pt(axeSz, 0), axePen);

		for (var i = -C.Grid.TickCount; i <= C.Grid.TickCount; i++)
		{
			if (i == 0) continue;
			var isLarge = i % C.GridGfx.LargeTickMultiple == 0;
			var pen = gfx.Pen(isLarge ? C.GridGfx.PenAxeTickLarge : C.GridGfx.PenAxeTickSmall);
			var t = i * C.Grid.TickSize;
			var sz = (isLarge ? C.GridGfx.AxeTickSizeLargePx : C.GridGfx.AxeTickSizeSmallPx) / gfx.Transform.Zoom;
			gfx.Line(new Pt(t, -sz), new Pt(t, sz), pen);
			gfx.Line(new Pt(-sz, t), new Pt(sz, t), pen);
		}
	}
}


static class GfxTweakExt
{
	private const float ArrowHalfBase = 8;
	private const float ArrowLength = 15;
	
	public static void LineArrowEnd(this Gfx gfx, Pt a, Pt b, Pen pen)
	{
		var u = (b - a).Norm();
		var v = new Pt(-u.Y, u.X);
		var halfBase = ArrowHalfBase / gfx.Transform.Zoom;
		var length = ArrowLength / gfx.Transform.Zoom;
		b -= u * length;
		var b0 = b + v * halfBase;
		var b1 = b - v * halfBase;
		var e = b + u * length;
		var brush = gfx.Res.Brush(Color.Black);
		var pts = new[] { b0, b1, e }.SelectToArray(e => e.ToPtF());
		gfx.Line(a, b, pen);
		gfx.Graphics.FillPolygon(brush, pts, System.Drawing.Drawing2D.FillMode.Winding);
	}
}


static class C
{
	// ********
	// * Zoom *
	// ********
	public static float[] ZoomLevels =
	{
		1f / 10,
		1f / 5,
		1f / 4,
		1f / 3,
		1f / 2,
		2f / 3,

		1,

		1.5f,
		2,
		3,
		4,
		6,
		10,
	};

	// ReSharper disable once CompareOfFloatsByEqualityOperator
	public static readonly int ZoomLevelOne = ZoomLevels.IndexOf(e => e == 1);

	// ********
	// * Grid *
	// ********
	public sealed record GridNfo(
		float TickSize,
		int TickCount
	);

	public static readonly GridNfo Grid = new(
		TickSize: 1.0f,
		TickCount: 10
	);
	public static class GridGfx
	{
		// @formatter:off
		public const			int		LargeTickMultiple = 5;
		public const			int		InitPaddingPx = 20;
		public static readonly	Color	BackColor = MkCol("FFFFFF");
		
		public static readonly	GPen	PenAxe = new(MkCol("000000"), 4, true);
		
		public static readonly	GPen	PenAxeTickLarge = new(MkCol("000000"), 2, true);
		public const			int		AxeTickSizeLargePx = 5;
		public static readonly	GPen	PenAxeTickSmall = new(MkCol("202020"), 1, true);
		public const			int		AxeTickSizeSmallPx = 4;
		
		public static readonly	GPen	PenGridLarge = new(MkCol("D8DFFF"), 1, true);
		public static readonly	GPen	PenGridSmall = new(MkCol("EFF2FF"), 1, true);
		// @formatter:on
	}
}
