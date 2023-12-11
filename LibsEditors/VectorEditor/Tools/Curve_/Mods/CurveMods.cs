using LinqVec.Utils;
using PowMaybe;
using VectorEditor.Model.Structs;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools.Curve_.Mods;


static class CurveMods
{
	public static Func<Curve, Maybe<Pt>, Curve> AddPoint() => (e, mp) => e.ApplyMod(new AddPointCurveMod(null), mp);
	public static Func<Curve, Maybe<Pt>, Curve> AddPoint(Maybe<Pt> mayStartPt) => mayStartPt.IsSome(out var startPt) switch
	{
		false => (e, _) => e,
		true => (e, mp) => e.ApplyMod(new AddPointCurveMod(startPt), mp)
	};
	public static Func<Curve, Maybe<Pt>, Curve> AddPoint(Pt startPt) => AddPoint(May.Some(startPt));
	public static Func<Curve, Maybe<Pt>, Curve> MovePoint(PointId id) => (e, mp) => e.ApplyMod(new MovePointCurveMod(id), mp);



	private interface ICurveMod;
	private sealed record AddPointCurveMod(Pt? StartPos) : ICurveMod;
	private sealed record MovePointCurveMod(PointId Id) : ICurveMod;
	private sealed record RemovePointCurveMod(int Idx) : ICurveMod;


	private static Curve ApplyMod(this Curve model, ICurveMod mod, Maybe<Pt> mp) =>
		mod switch
		{
			AddPointCurveMod { StartPos: var startPos } => mp.IsSome(out var p) switch
			{
				false => model,
				true => model with { Pts = model.Pts.Add(CurvePt.Make(startPos, p)) },
			},
			MovePointCurveMod { Id: var id } => mp.IsSome(out var p) switch
			{
				false => model,
				true => model with { Pts = model.Pts.ChangeIdx(id.Idx, e => e.Move(id.Type, p)) },
			},
			RemovePointCurveMod { Idx: var idx }
				=> model with { Pts = model.Pts.RemoveIdx(idx) },
			_ => throw new ArgumentException()
		};

	private static CurvePt Move(this CurvePt pt, PointType type, Pt pos) => type switch
	{
		PointType.Point => pt.MovePoint(pos),
		PointType.LeftHandle => pt.MoveLeftHandle(pos),
		PointType.RightHandle => pt.MoveRightHandle(pos),
		_ => throw new ArgumentException()
	};

	private static CurvePt MovePoint(this CurvePt p, Pt pos)
	{
		var delta = pos - p.P;
		return new CurvePt(pos, p.HLeft + delta, p.HRight + delta);
	}

	private static CurvePt MoveLeftHandle(this CurvePt p, Pt pos)
	{
		var delta = pos - p.P;
		return new CurvePt(p.P, pos, p.P - delta);
	}
	private static CurvePt MoveRightHandle(this CurvePt p, Pt pos)
	{
		var delta = pos - p.P;
		return new CurvePt(p.P, p.P - delta, pos);
	}
}