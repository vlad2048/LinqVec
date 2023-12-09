using LinqVec.Utils;
using PowMaybe;
using VectorEditor.Model.Structs;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Model;


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

	//public static Maybe<>
}
