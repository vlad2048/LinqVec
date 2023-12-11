using System.Text.Json.Serialization;
using Geom;
using LinqVec.Structs;
using VectorEditor.Model.Structs;

namespace VectorEditor.Model;




public sealed record Doc(
	Layer[] Layers
) : IDoc
{
	public static readonly Doc Empty = new(new[] { Layer.Empty() });

	[JsonIgnore]
	public IId[] AllObjects => Layers.OfType<IId>().Concat(Layers.SelectMany(e => e.Objects)).ToArray();
}


public sealed record Layer(
	Guid Id,
	IVisualObjSer[] Objects
) : IId
{
	public static Layer Empty() => new(Guid.NewGuid(), Array.Empty<IVisualObjSer>());
}

public sealed record Curve(
	Guid Id,
	CurvePt[] Pts
) : IVisualObjSer
{
	public static Curve Empty() => new(
		Guid.NewGuid(),
		Array.Empty<CurvePt>()
	);

	public R BoundingBox => this.GetDrawPoints().GetBBox();
	public double DistanceToPoint(Pt pt) => this.GetDrawPoints().DistanceToPoint(pt);

	public override string ToString() => $"points:{Pts.Length}";
}




[JsonDerivedType(typeof(Curve), typeDiscriminator: "Curve")]
public interface IVisualObjSer : IVisualObj;





/*
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
*/

