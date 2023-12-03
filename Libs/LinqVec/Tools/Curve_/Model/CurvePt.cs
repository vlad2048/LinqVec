namespace LinqVec.Tools.Curve_.Model;

enum PointType
{
    Point,
    LeftHandle,
    RightHandle
}

sealed record PointId(int Idx, PointType Type);

sealed record CurvePt(
    Pt P,
    Pt HLeft,
    Pt HRight
)
{
	public static CurvePt Make(Pt? startPt, Pt endPt) => startPt switch
	{
		null => new CurvePt(endPt, endPt, endPt),
		not null => new CurvePt(startPt.Value, startPt.Value - (endPt - startPt.Value), endPt)
	};

	public bool HasHandles => HLeft != P || HRight != P;
}


static class CurvePtExt
{
	public static Pt GetPt(this CurvePt pt, PointType type) => type switch
	{
		PointType.Point => pt.P,
		PointType.LeftHandle => pt.HLeft,
		PointType.RightHandle => pt.HRight,
		_ => throw new ArgumentException()
	};

	/*public static HandleSide Neg(this HandleSide side) => side switch
	{
		HandleSide.Left => HandleSide.Right,
		HandleSide.Right => HandleSide.Left,
		_ => throw new ArgumentException()
	};
	public static Pt GetHandle(this CurvePt p, HandleSide side) => side switch
	{
		HandleSide.Left => p.HLeft,
		HandleSide.Right => p.HRight,
		_ => throw new ArgumentException()
	};*/
}
