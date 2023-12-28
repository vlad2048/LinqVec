using System.Text.Json.Serialization;
using Geom;
using LinqVec;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using VectorEditor.Model.Structs;

namespace VectorEditor.Model;



[JsonDerivedType(typeof(Curve), typeDiscriminator: "Curve")]
public interface IObj : IId
{
	[JsonIgnore]
	R BoundingBox { get; }
	double DistanceToPoint(Pt pt);
}

public sealed record Doc(
	Layer[] Layers,
	Guid ActiveLayer
) : IDoc
{
	public static Doc Empty() => new([new Layer(Guid.Empty, [])], Guid.Empty);

	[JsonIgnore]
	public IId[] AllObjects => Layers.OfType<IId>().Concat(Layers.SelectMany(e => e.Objects)).ToArray();

	public override string ToString() => "[" + Layers.SelectMany(e => e.Objects).JoinText() + "]";

	public string GetUndoRedoStr()
	{
		var curves = Layers.SelectMany(e => e.Objects).OfType<Curve>().ToArray();
		var str = curves.Select(e => $"{e.Pts.Length}").JoinText(",");
		return $"[{str}]";
	}
}


public sealed record Layer(
	Guid Id,
	IObj[] Objects
) : IId
{
	public static Layer Empty() => new(Guid.NewGuid(), []);
}

public sealed record Curve(
	Guid Id,
	CurvePt[] Pts
) : IObj
{
	public static Curve Empty() => new(Guid.NewGuid(), []);

	public R BoundingBox => this.GetDrawPoints().GetBBox();
	public double DistanceToPoint(Pt pt) => this.GetDrawPoints().DistanceToPoint(pt);

	public override string ToString() => $"Curve({Pts.Select(e => $"({e})").JoinText(",")})";
}


static class DocUtils
{
	public static Doc SetCurve(Doc doc, Curve curve) =>
		doc with {
			Layers = [
				SetCurve(doc.Layers[0], curve),
				..doc.Layers.Skip(1)
			]
		};

	private static Layer SetCurve(Layer layer, Curve curve) => layer with { Objects = layer.Objects.AddSet(curve) };

	private static T[] AddSet<T>(this T[] xs, T x) where T : IId
	{
		var idx = xs.IndexOf(e => e.Id == x.Id);
		var list = xs.ToList();
		if (idx == -1) return list.Append(x).ToArray();
		list[idx] = x;
		return list.ToArray();
	}
}


/*
static class Entities
{
	public static IPtr<Doc, Curve> CurveCreate(this Model<Doc> model, Disp toolD) => new PtrCreate<Doc, Curve, Guid>(model, Curve, toolD);


	private static readonly IObjDesc<Doc, Curve, Guid> Curve = new CurveDesc();
	private sealed class CurveDesc : IObjDesc<Doc, Curve, Guid>
	{
		public Curve Make(Guid objId) => new(objId, []);
		public bool Contains(Doc doc, Guid loc, Guid objId) => doc.ObjContains<Curve>(loc, objId);
		public Guid LocStrat(Doc doc) => doc.ActiveLayer;
		public Doc Add(Doc doc, Guid loc, Curve obj) => doc.ObjAdd(loc, obj);
		public Doc Del(Doc doc, Guid loc, Guid objId) => doc.ObjDel(loc, objId);
		public Curve Get(Doc doc, Guid loc, Guid objId) => doc.ObjGet<Curve>(loc, objId);
		public Doc Set(Doc doc, Guid loc, Curve obj) => doc.ObjSet(loc, obj);
	}



	private static bool ObjContains<O>(this Doc doc, Guid layerId, Guid objId) where O : IObj => (
		from layer in doc.Layers.MayGet(layerId)
		from obj in layer.Objects.MayGet(objId)
		where obj is O
		select obj
	).IsSome;
	private static Doc ObjAdd(this Doc doc, Guid layerId, IObj obj) => doc.ChangeLayer(layerId, objs => objs.Add(obj));
	private static Doc ObjDel(this Doc doc, Guid layerId, Guid objId) => doc.ChangeLayer(layerId, objs => objs.Del(objId));
	private static O ObjGet<O>(this Doc doc, Guid layerId, Guid objId) where O : IObj => (O)doc.Layers.Get(layerId).Objects.Get(objId);
	private static Doc ObjSet(this Doc doc, Guid layerId, IObj obj) => doc.ChangeLayer(layerId, objs => objs.Set(obj));

	private static Doc ChangeLayer(this Doc doc, Guid layerId, Func<IObj[], IObj[]> fun) => doc with { Layers = doc.Layers.Set(layerId, layer => layer with { Objects = fun(layer.Objects) }) };
}
*/