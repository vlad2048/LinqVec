using Geom;
using LinqVec.Utils;
using VectorEditor.Model.Structs;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools.Curve_.Mods;


static class CurveMods
{
	public static Func<Curve, Option<Pt>, Curve> AddPoint() => (e, mp) => e.ApplyMod(new AddPointCurveMod(null), mp);

	public static Func<Curve, Option<Pt>, Curve> AddPoint(Option<Pt> mayStartPt) => mayStartPt.Match<Func<Curve, Option<Pt>, Curve>>(
		startPt => (e, mp) => e.ApplyMod(new AddPointCurveMod(startPt), mp),
		() => (e, _) => e
	);
	public static Func<Curve, Option<Pt>, Curve> AddPoint(Pt startPt) => AddPoint(Some(startPt));
	public static Func<Curve, Option<Pt>, Curve> MovePoint(PointId id) => (e, mp) => e.ApplyMod(new MovePointCurveMod(id), mp);



	private interface ICurveMod;
	private sealed record AddPointCurveMod(Pt? StartPos) : ICurveMod;
	private sealed record MovePointCurveMod(PointId Id) : ICurveMod;
	private sealed record RemovePointCurveMod(int Idx) : ICurveMod;


	private static Curve ApplyMod(this Curve model, ICurveMod mod, Option<Pt> mp) =>
	mod switch
	{
			AddPointCurveMod { StartPos: var startPos } => mp.Match(
				p => model with { Pts = model.Pts.Add(CurvePt.Make(startPos, p)) },
				() => model
			),
			MovePointCurveMod { Id: var id } => mp.Match(
				p => model with { Pts = model.Pts.ChangeIdx(id.Idx, e => e.Move(id.Type, p)) },
				() => model
			),
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