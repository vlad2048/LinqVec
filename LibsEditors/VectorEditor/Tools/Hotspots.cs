using Geom;
using LinqVec.Logic;
using LinqVec.Tools.Acts.Structs;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools;

static class Hotspots
{
	public static readonly Hotspot<Pt> Anywhere = new(
		Option<Pt>.Some,
		null
	);

	public static Hotspot<PointId> CurvePoint(IMouseModder<Curve> curve) => new(
		p => curve.Get().GetClosestPointTo(p, C.ActivateMoveMouseDistance),
		null
	);
}