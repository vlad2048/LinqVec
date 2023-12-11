using LinqVec.Structs;
using LinqVec.Utils;
using PowMaybe;
using VectorEditor.Model.Structs;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Model;


static class CurveOps
{
	public static Doc AddCurve(this Doc doc, Curve curve) => doc.WithLayers(doc.Layers.ChangeId(doc.Layers[0].Id, layer => layer.WithObjects(layer.Objects.AddId(curve))));


	private static Doc WithLayers(this Doc m, Layer[] xs) => m with { Layers = xs };
	private static Layer WithObjects(this Layer m, IVisualObjSer[] xs) => m with { Objects = xs };



	private sealed record PtNfo(PointId Id, double Distance);

	public static Maybe<PointId> GetClosestPointTo(this Curve model, Pt pt, double threshold)
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

	public static Maybe<IVisualObjSer> GetObjectAt(this Doc doc, Pt pt)
	{
		var objs = doc.AllObjects.OfType<IVisualObjSer>().ToArray();
		return objs.Length switch
		{
			0 => May.None<IVisualObjSer>(),
			_ => objs
				.Select(obj => (obj, obj.DistanceToPoint(pt)))
				.Where(t => t.Item2 < C.ActivateMoveMouseDistance)
				.OrderBy(t => t.Item2)
				.Select(t => t.obj)
				.FirstOrMaybe()
		};
	}
}
