using LinqVec.Utils;
using VectorEditor._Model;
using VectorEditor._Model.Interfaces;
using VectorEditor._Model.Structs;
using VectorEditor._Model.Structs.Enums;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Cmds.Utils;
using ReactiveVars;

namespace VectorEditor.Tools;

static class Hotspots
{
	public const string AnywhereId = nameof(Anywhere);
	public const string CurvePointId = nameof(CurvePoint);

	public static readonly HotspotNfo<Unit> Anywhere = new(
		AnywhereId,
		_ => Unit.Default
	);

	public static readonly HotspotNfo<Unit> AnywhereNeg = new(
		AnywhereId,
		pt => pt.X switch
		{
			< 0 => Unit.Default,
			_ => None
		}
	);

	public static HotspotNfo<PointId> CurvePoint(IRoVar<Curve> curve, bool excludeLast) => new(
		CurvePointId,
		p => excludeLast switch
		{
			false => curve.V.GetClosestPointTo(p, C.ActivateMoveMouseDistance),
			true => curve.V.GetClosestPointToButLast(p, C.ActivateMoveMouseDistance),
		}
	);

	public static HotspotNfo<StartOrEnd> CurveExtremity(Doc doc, Guid curveId) => new(
		nameof(CurveExtremity),
		p =>
			from curve in doc.GetObject<Curve>(curveId)
			from extremity in curve.GetExtremityAt(p, C.ActivateMoveMouseDistance)
			select extremity
	);



	public static HotspotNfo<Guid> Object(Doc doc) => new(
		nameof(Object),
		p => doc.GetObjectAt(p).Map(e => e.Id)
	);

	public static HotspotNfo<Guid> Object<T>(Doc doc) where T : IObj => new(
		$"{nameof(Object)}<{typeof(T).Name}>",
		p => doc.GetObjectAt(p).OfType<IObj, T>().Map(e => e.Id)
	);

	public static HotspotNfo<Unit> Object(Doc doc, Guid objId) => new(
		nameof(Object),
		p => doc.GetObjectAt(p).FirstOrOption(e => e.Id == objId).Map(_ => Unit.Default)
	);
}