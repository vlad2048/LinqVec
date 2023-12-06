using LinqVec.Utils;
using PowMaybe;
using VectorEditor.Model.Structs;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools.Curve_.Mods;

interface ICurveMod;
sealed record AddPointCurveMod(Pt? StartPos) : ICurveMod;
sealed record MovePointCurveMod(PointId Id) : ICurveMod;
sealed record RemovePointCurveMod(int Idx) : ICurveMod;

static class CurveMods
{
	public static Func<CurveModel, Maybe<Pt>, CurveModel> AddPoint() => (e, mp) => e.ApplyMod(new AddPointCurveMod(null), mp);
	public static Func<CurveModel, Maybe<Pt>, CurveModel> AddPoint(Pt startPt) => (e, mp) => e.ApplyMod(new AddPointCurveMod(startPt), mp);
	public static Func<CurveModel, Maybe<Pt>, CurveModel> MovePoint(PointId id) => (e, mp) => e.ApplyMod(new MovePointCurveMod(id), mp);

	public static Func<CurveModel, Maybe<Pt>, CurveModel> If(this Func<CurveModel, Maybe<Pt>, CurveModel> fun, bool on) => on switch
	{
		false => (e, _) => e,
		true => fun
	};
}

static class CurveModExt
{
	public static CurveModel ApplyMod(this CurveModel model, ICurveMod mod, Maybe<Pt> mp) =>
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