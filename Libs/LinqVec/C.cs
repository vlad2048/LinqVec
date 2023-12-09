using LinqVec.Structs;
using PowBasics.CollectionsExt;

namespace LinqVec;


static class GridNfoExt
{
	public static R BBox(this C.GridNfo g)
	{
		var x = (g.TickCount + 1) * g.TickSize;
		return new(
			new Pt(-x, -x),
			new Pt(x, x)
		);
	}
}

public static class CBase
{
	// ***********
	// * Cursors *
	// ***********
	public static class Cursors
	{
		public static readonly Cursor HandOpened = CUtils.LoadCursor(Resource.hand_opened);
		public static readonly Cursor HandClosed = CUtils.LoadCursor(Resource.hand_closed);
		public static readonly Cursor Pen = CUtils.LoadCursor(Resource.pen);
		public static readonly Cursor BlackArrowSmall = CUtils.LoadCursor(Resource.black_arrow_small);
		public static readonly Cursor BlackArrow = CUtils.LoadCursor(Resource.black_arrow);
		public static readonly Cursor BlackArrowHold = CUtils.LoadCursor(Resource.black_arrow_hold);
	}
}

static class C
{
	// *********
	// * Fonts *
	// *********
	public static class Fonts
	{
		public static readonly Font MonoHeader = new("Consolas", 8, FontStyle.Regular);
		public static readonly Font MonoValue = new("Consolas", 8, FontStyle.Bold);
	}

	// **********
	// * KeyMap *
	// **********
	public static class KeyMap
	{
		public static readonly Keys PanZoom = Keys.Space;
	}

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
		TickSize:	1.0f,
		TickCount:	10
	);
	public static class GridGfx
	{
		// @formatter:off
		public const			int		LargeTickMultiple = 5;
		public const			int		InitPaddingPx = 20;
		public static readonly	Color	BackColor = MkCol(0xFFFFFF);
		public const			float	ArrowHalfBase = 8;
		public const			float	ArrowLength = 15;

		public static readonly	GPen	PenAxe = new(0x000000, 4, true);
		
		public static readonly	GPen	PenAxeTickLarge = new(0x000000, 2, true);
		public const			int		AxeTickSizeLargePx = 5;
		public static readonly	GPen	PenAxeTickSmall = new(0x202020, 1, true);
		public const			int		AxeTickSizeSmallPx = 4;
		
		public static readonly	GPen	PenGridLarge = new(0xD8DFFF, 1, true);
		public static readonly	GPen	PenGridSmall = new(0xEFF2FF, 1, true);
		// @formatter:on
	}

	
}


file static class CUtils
{
	public static Cursor LoadCursor(byte[] data)
	{
		using var ms = new MemoryStream(data);
		return new Cursor(ms);
	}
}