using LinqVec.Structs;
using System.Drawing;

namespace VectorEditor;

static class C
{
	// ***********
	// * Markers *
	// ***********
	public static class Markers
	{
		// @formatter:off
		public const int Radius = 2;
		public static readonly GPen PointPen = new(0x4F80FF, 1, true);
		public static readonly Color PointBrush = MkCol(0x4F80FF);
		public static readonly GPen PointPenDone = new(0xAAC2FF, 1, true);
		public static readonly Color PointBrushDone = MkCol(0xAAC2FF);
		// @formatter:on
	}

	// *********
	// * Curve *
	// *********
	public const double ActivateMoveMouseDistance = 1;
	public static readonly GPen PenCurve = new(0x000000, 2, true);
}