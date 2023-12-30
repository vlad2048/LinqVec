﻿using LinqVec.Utils;
using LinqVec.Tools.Cmds;
using VectorEditor._Model;
using VectorEditor._Model.Interfaces;
using VectorEditor._Model.Structs;

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


	public static Hotspot<Guid> Object(Doc doc) => new(
		nameof(Object),
		p => doc.GetObjectAt(p).Map(e => e.Id)
	);

	public static Hotspot<Guid> Object<T>(Doc doc) where T : IObj => new(
		$"{nameof(Object)}<{typeof(T).Name}>",
		p => doc.GetObjectAt(p).OfType<IObj, T>().Map(e => e.Id)
	);
}