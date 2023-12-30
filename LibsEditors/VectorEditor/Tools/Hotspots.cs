using LinqVec.Utils;
using LinqVec.Tools.Cmds;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools;

static class Hotspots
{
	public const string AnywhereId = nameof(Anywhere);
	public const string CurvePointId = nameof(CurvePoint);

	public static readonly Hotspot<Unit> Anywhere = new(
		AnywhereId,
		_ => Unit.Default
	);

	public static readonly Hotspot<Unit> AnywhereNeg = new(
		AnywhereId,
		pt => pt.X switch
		{
			< 0 => Unit.Default,
			_ => None
		}
	);

	public static Hotspot<PointId> CurvePoint(Curve curve, bool excludeLast) => new(
		CurvePointId,
		p => excludeLast switch
		{
			false => curve.GetClosestPointTo(p, C.ActivateMoveMouseDistance),
			true => curve.GetClosestPointToButLast(p, C.ActivateMoveMouseDistance),
		}
	);

	/*public static Hotspot<Curve> Curve(Model<Doc> doc) => new(
		nameof(Curve),
		p => doc.Cur.V.GetObjectAt(p).OfType<IObj, Curve>()
	);*/

	public static Hotspot<Guid> Object(Doc doc) => new(
		nameof(Object),
		p => doc.GetObjectAt(p).Map(e => e.Id)
	);

	public static Hotspot<Guid> Object<T>(Doc doc) where T : IObj => new(
		$"{nameof(Object)}<{typeof(T).Name}>",
		p => doc.GetObjectAt(p).OfType<IObj, T>().Map(e => e.Id)
	);



	/*
	public static Hotspot<Curve> CurveSpecific(Model<Doc> doc, Curve curve) => new(
		nameof(CurveSpecific),
		p => doc.Cur.GetObjectAt(p).OfType<IObj, Curve>().Where(e => e == curve),
		null
	);

	public static Hotspot<Curve> CurveExcept(Model<Doc> doc, Curve curve) => new(
		nameof(CurveExcept),
		p => doc.Cur.GetObjectAt(p).OfType<IObj, Curve>().Where(e => e != curve),
		null
	);

	public static Hotspot<Curve> Curve(Model<Doc> doc) => new(
		nameof(Curve),
		p => doc.Cur.GetObjectAt(p).OfType<IObj, Curve>(),
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
	*/
}