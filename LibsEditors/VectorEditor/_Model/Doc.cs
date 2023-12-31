using System.Text.Json.Serialization;
using Geom;
using LinqVec.Interfaces;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using VectorEditor._Model.Interfaces;
using VectorEditor._Model.Structs;

namespace VectorEditor._Model;

public sealed record Doc(
	Layer[] Layers,
	Guid ActiveLayer
)
{
	public static Doc Empty() => new([new Layer(Guid.Empty, [])], Guid.Empty);

	[JsonIgnore]
	public IId[] AllObjects => Layers.OfType<IId>().Concat(Layers.SelectMany(e => e.Objects)).ToArray();

	public override string ToString() => "[" + Layers.SelectMany(e => e.Objects).JoinText() + "]";

	//public string GetUndoRedoStr()
	//{
	//	var curves = Layers.SelectMany(e => e.Objects).OfType<Curve>().ToArray();
	//	var str = curves.Select(e => $"{e.Pts.Length}").JoinText(",");
	//	return $"[{str}]";
	//}
}


// ********
// * Mods *
// ********
static class DocMods
{
	public static Doc MoveSelection(this Doc doc, Arr<Guid> selObjIds, Pt delta) =>
		selObjIds.Aggregate(
			doc,
			(acc, id) => MoveSelection(acc, id, delta)
		);

	private static Doc MoveSelection(Doc doc, Guid selObjId, Pt delta) =>
		doc with
		{
			Layers = doc.Layers
				.SelectToArray(
					layer => layer with
					{
						Objects = layer.Objects
							.SelectToArray(obj => (obj.Id == selObjId) switch {
								false => obj,
								true => MoveSelection(obj, delta)
							})
					}
				)
		};

	private static IObj MoveSelection(IObj obj, Pt delta)
	{
		if (obj is not Curve curve) throw new ArgumentException();
		return curve with
		{
			Pts = curve.Pts.SelectToArray(e => Move(e, delta))
		};
	}

	private static CurvePt Move(CurvePt p, Pt delta) => new(
		p.P + delta,
		p.HLeft + delta,
		p.HRight + delta
	);
}


// *********
// * Utils *
// *********
static class DocUtils
{
	public static Option<T> GetObject<T>(this Doc doc, Guid objId) where T : IObj =>
		doc.GetObjects(new[] { objId }).FirstOrOption().OfType<IObj, T>();

	public static IObj[] GetObjects(this Doc doc, Arr<Guid> objIds) => (
		from layer in doc.Layers
		from obj in layer.Objects
		where objIds.Contains(obj.Id)
		select obj
	).ToArray();

	public static Doc DeleteObjects(this Doc doc, Arr<Guid> objIds) =>
		doc with
		{
			Layers = doc.Layers.SelectToArray(
				layer => layer with
				{
					Objects = layer.Objects.WhereNotToArray(e => objIds.Contains(e.Id))
				}
			)
		};

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

