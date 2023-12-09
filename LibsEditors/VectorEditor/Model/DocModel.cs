using System.Text.Json.Serialization;
using LinqVec.Structs;
using PowBasics.CollectionsExt;
using VectorEditor.Model.Structs;

namespace VectorEditor.Model;

// Doc
// ===
public sealed record DocModel(
	LayerModel[] Layers
)
{
	public static readonly DocModel Empty = new(new[] { LayerModel.Empty() });
}


// Layer
// =====
[JsonDerivedType(typeof(CurveModel), typeDiscriminator: "Curve")]
public interface ILayerObject : IId;

public sealed record LayerModel(
	Guid Id,
	ILayerObject[] Objects
) : IId
{
	public static LayerModel Empty() => new(
		Guid.NewGuid(),
		Array.Empty<ILayerObject>()
	);
}


// Curve
// =====
public sealed record CurveModel(
	Guid Id,
	CurvePt[] Pts
) : ILayerObject
{
	public static CurveModel Empty() => new(
		Guid.NewGuid(),
		Array.Empty<CurvePt>()
	);

	public override string ToString() => "Curve(" + Pts.SelectToArray(e => $"{(int)e.P.X},{(int)e.P.Y}").JoinText() + ")";
}



public interface ICurveModelEditState;

// State:	Pts[0], ..., Pts[n-1]

// - draw Marker @ MousePos
// - draw RedLine @ (Pts[n-1].P, Pts[n-1].HRight) -> (MousePos, MousePos)
// - draw Handles @ Pts[n-1]
// - finish: MouseDown(DownPos <- MousePos)
public sealed record AddPointPre_CurveModelEditState : ICurveModelEditState;

// - draw Marker @ DownPos
// - draw RedLine @ (Pts[n-1].P, Pts[n-1].HRight) -> (DownPos-(MousePos-DownPos), DownPos)
// - draw Handles @ (DownPos-(MousePos-DownPos), DownPos, DownPos+(MousePos-DownPos))
// - finish: MouseUp => Model.AddPoint(DownPos-(MousePos-DownPos), DownPos, DownPos+(MousePos-DownPos))
public sealed record AddPointHandleDrag_CurveModelEditState(Pt DownPos) : ICurveModelEditState;

// State:	Pts[0], ..., Pts[n-1], Pts[n]

