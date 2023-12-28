using Geom;
using LinqVec.Utils;
using VectorEditor.Model.Structs;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Model;


static class CurveOps
{
	public static IObj[] GetObjects(this Doc doc, Guid[] objIds) => (
		from layer in doc.Layers
		from obj in layer.Objects
		where objIds.Contains(obj.Id)
		select obj
	).ToArray();


	private sealed record PtNfo(PointId Id, double Distance);

	public static Option<PointId> GetClosestPointTo(this Curve model, Pt pt, double threshold)
	{
		PtNfo Mk(CurvePt mp, int idx, PointType type) => new(new PointId(idx, type), (mp.GetPt(type) - pt).Length);

		Option<PointId> For(PointType type) =>
			model.Pts
				.Select((e, i) => Mk(e, i, type))
				.OrderByDescending(e => e.Distance)
				.Where(e => e.Distance < threshold)
				.Select(e => e.Id)
				.FirstOrOption();

		return OptionExt.AggregateArr(
			For(PointType.Point),
			For(PointType.LeftHandle),
			For(PointType.RightHandle)
		);
	}

	public static Option<PointId> GetClosestPointToButLast(this Curve model, Pt pt, double threshold)
	{
		PtNfo Mk(CurvePt mp, int idx, PointType type) => new(new PointId(idx, type), (mp.GetPt(type) - pt).Length);

		Option<PointId> For(PointType type) =>
			model.Pts
				.Select((e, i) => Mk(e, i, type))
				.SkipLast(1)
				.OrderByDescending(e => e.Distance)
				.Where(e => e.Distance < threshold)
				.Select(e => e.Id)
				.FirstOrOption();

		return OptionExt.AggregateArr(
			For(PointType.Point),
			For(PointType.LeftHandle),
			For(PointType.RightHandle)
		);
	}

	public static Pt[] GetDrawPoints(this Curve model) =>
		model.Pts
			.SelectMany(p => new[]
			{
				p.HLeft,
				p.P,
				p.HRight
			})
			.Skip(1)
			.SkipLast(1)
			.ToArray();

	public static Option<IObj> GetObjectAt(this Doc doc, Pt pt)
	{
		var objs = doc.AllObjects.OfType<IObj>().ToArray();
		return objs.Length switch
		{
			0 => Option<IObj>.None,
			_ => objs
				.Select(obj => (obj, obj.DistanceToPoint(pt)))
				.Where(t => t.Item2 < C.ActivateMoveMouseDistance)
				.OrderBy(t => t.Item2)
				.Select(t => t.obj)
				.FirstOrOption()
		};
	}
}
