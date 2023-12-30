using System.Text.Json.Serialization;
using Geom;

namespace VectorEditor._Model.Structs;

public sealed record CurvePt(
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

	[JsonIgnore]
	public bool HasHandles => HLeft != P || HRight != P;
}
