using LinqVec.Utils;
using PowBasics.CollectionsExt;
using PowMaybe;
using VectorEditor.Model.Structs;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Model;



public sealed record CurveModel(
	Guid Id,
	CurvePt[] Pts
) : ILayerObject
{
	public static CurveModel Empty() => new(
		Guid.NewGuid(),
		Array.Empty<CurvePt>()
	);

	public override string ToString() => "Curve(" + Pts.SelectToArray(e => $"{(int)e.P.X},{(int)e.P.Y}").JoinText() + ")";
}

static class CurveModelOps
{
	private sealed record PtNfo(PointId Id, double Distance);

	public static Pt GetPointById(this CurveModel model, PointId id) => model.Pts[id.Idx].GetPt(id.Type);

	public static Maybe<PointId> GetClosestPointTo(this CurveModel model, Pt pt, double threshold)
	{
		PtNfo Mk(CurvePt mp, int idx, PointType type) => new(new PointId(idx, type), (mp.GetPt(type) - pt).Length);

		Maybe<PointId> For(PointType type) =>
			model.Pts
				.Select((e, i) => Mk(e, i, type))
				.OrderByDescending(e => e.Distance)
				.Where(e => e.Distance < threshold)
				.Select(e => e.Id)
				.FirstOrMaybe();

		return MaybeUtils.Aggregate(
			For(PointType.Point),
			For(PointType.LeftHandle),
			For(PointType.RightHandle)
		);
	}
}



/*static class CurveModelOps
{
	public static ISmartId<CurveModel> SmartId(this CurveModel curveModel, ModelMan<DocModel> mm) => new SmartId<DocModel, CurveModel>(
		curveModel.Id,
		mm,
		find: m => m.Curves.SingleOrMaybe(e => e.Id == curveModel.Id),
		set: (m, e) => m.WithCurves(m.Curves.SetId(curveModel.Id, e)),
		delete: m => m.WithCurves(m.Curves.RemoveId(curveModel.Id))
	);

	public static DocModel WithCurves(this DocModel model, CurveModel[] curves) => model with { Curves = curves };

	private sealed record PtNfo(PointId Id, double Distance);

	public static Pt GetPointById(this CurveModel model, PointId id) => model.Pts[id.Idx].GetPt(id.Type);

	public static Maybe<PointId> GetClosestPointTo(this CurveModel model, Pt pt, double threshold)
	{
		PtNfo Mk(CurvePt mp, int idx, PointType type) => new(new PointId(idx, type), (mp.GetPt(type) - pt).Length);

		Maybe<PointId> For(PointType type) =>
			model.Pts
				.Select((e, i) => Mk(e, i, type))
				.OrderByDescending(e => e.Distance)
				.Where(e => e.Distance < threshold)
				.Select(e => e.Id)
				.FirstOrMaybe();

		return MaybeUtils.Aggregate(
			For(PointType.Point),
			For(PointType.LeftHandle),
			For(PointType.RightHandle)
		);
	}


	public static CurveModel ApplyMod(this CurveModel model, ICurveMod mod, Maybe<Pt> pos) => pos.IsSome(out var p) switch
	{
		true => model.ApplyMod(mod, p),
		false => model
	};

	public static CurveModel ApplyMod(this CurveModel model, ICurveMod mod, Pt pos) => mod switch
	{
		NoneCurveMod
			=> model,
		AddPointCurveMod { StartPos: var startPos }
			=> model with { Pts = model.Pts.Add(CurvePt.Make(startPos, pos)) },
		MovePointCurveMod { Id: var id }
			=> model with { Pts = model.Pts.ChangeIdx(id.Idx, e => e.Move(id.Type, pos)) },
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
}*/
