using System.Text;
using Geom;
using LinqVec.Utils;
using VectorEditor._Model;
using VectorEditor._Model.Interfaces;
using VectorEditor._Model.Structs;
using VectorEditor._Model.Structs.Enums;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Cmds.Utils;
using PowBasics.CollectionsExt;
using ReactiveVars;

namespace VectorEditor.Tools;

[Flags]
enum CurvePointType
{
	First = 1,
	Middle = 2,
	Last = 4,
	All = First | Middle | Last
}

static class Hotspots
{
	public static readonly HotspotNfo<Pt> Anywhere = new(
		nameof(Anywhere),
		p => p
	);

	public static readonly HotspotNfo<Unit> AnywhereNeg = new(
		nameof(AnywhereNeg),
		pt => pt.X switch
		{
			< 0 => Unit.Default,
			_ => None
		}
	);

	public static HotspotNfo<PointId> CurvePoint(IRoVar<Curve> curve, CurvePointType type) => new(
		$"{nameof(CurvePoint)}({type.Fmt()})",
		p => curve.V.GetClosestPointTo(p, C.ActivateMoveMouseDistance).Where(e => MatchesType(type, e, curve.V.Pts.Length))
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




	private static bool MatchesType(CurvePointType type, PointId pointId, int pointCount)
	{
		var idx = pointId.Idx;
		if (idx == 0)
			return type.HasFlag(CurvePointType.First);
		if (idx == pointCount - 1)
			return type.HasFlag(CurvePointType.Last);
		return type.HasFlag(CurvePointType.Middle);
	}

	private static string Fmt(this CurvePointType t)
	{
		if (t == 0) return "None";
		if (t == CurvePointType.All) return "All";
		var l = new List<string>();
		if (t.HasFlag(CurvePointType.First)) l.Add("Fst");
		if (t.HasFlag(CurvePointType.Middle)) l.Add("Mid");
		if (t.HasFlag(CurvePointType.Last)) l.Add("Lst");
		return l.JoinText("|");
	}
}