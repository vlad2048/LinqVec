using Geom;
using VectorEditor.Model.Structs;

namespace VectorEditor.Tools.Curve_.Structs;

enum PointType
{
	Point,
	LeftHandle,
	RightHandle
}

sealed record PointId(int Idx, PointType Type);

static class CurvePtExt
{
	public static Pt GetPt(this CurvePt pt, PointType type) => type switch
	{
		PointType.Point => pt.P,
		PointType.LeftHandle => pt.HLeft,
		PointType.RightHandle => pt.HRight,
		_ => throw new ArgumentException()
	};
}
