using Geom;
using LinqVec.Logic;
using LinqVec.Tools.Acts;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools;

static class Hotspots
{
	public static readonly Hotspot<Unit> Anywhere = new(
		nameof(Anywhere),
		_ => Unit.Default,
		null
	);

	public static Hotspot<Curve> CurveSpecific(Model<Doc> doc, Curve curve) => new(
		nameof(CurveSpecific),
		p => doc.V.GetObjectAt(p).OfType<IVisualObjSer, Curve>().Where(e => e == curve),
		null
	);

	public static Hotspot<Curve> CurveExcept(Model<Doc> doc, Curve curve) => new(
		nameof(CurveExcept),
		p => doc.V.GetObjectAt(p).OfType<IVisualObjSer, Curve>().Where(e => e != curve),
		null
	);

	public static Hotspot<Curve> Curve(Model<Doc> doc) => new(
		nameof(Curve),
		p => doc.V.GetObjectAt(p).OfType<IVisualObjSer, Curve>(),
		null
	);

	public static Hotspot<PointId> CurvePoint(IMouseModder<Curve> curve) => new(
		nameof(CurvePoint),
		p => curve.Get().GetClosestPointTo(p, C.ActivateMoveMouseDistance),
		null
	);

	public static Hotspot<PointId> CurvePointButLast(IMouseModder<Curve> curve) => new(
		nameof(CurvePointButLast),
		p => curve.Get().GetClosestPointToButLast(p, C.ActivateMoveMouseDistance),
		null
	);


	private static Option<U> OfType<T, U>(this Option<T> opt) where U : T =>
		opt.BiBind(
			v => v switch
			{
				U u => Option<U>.Some(u),
				_ => Option<U>.None
			},
			() => Option<U>.None
		);
}