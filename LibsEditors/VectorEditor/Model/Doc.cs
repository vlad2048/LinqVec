using System.Text.Json.Serialization;
using Geom;
using LinqVec.Structs;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using VectorEditor.Model.Structs;

namespace VectorEditor.Model;




public sealed record Doc(
	Layer[] Layers
) : IDoc
{
	public static readonly Doc Empty = new(new[] { Layer.Empty() });

	[JsonIgnore]
	public IId[] AllObjects => Layers.OfType<IId>().Concat(Layers.SelectMany(e => e.Objects)).ToArray();

	public override string ToString() => "[" + Layers.SelectMany(e => e.Objects).JoinText() + "]";
}


public sealed record Layer(
	Guid Id,
	IVisualObjSer[] Objects
) : IId
{
	public static Layer Empty() => new(Guid.NewGuid(), System.Array.Empty<IVisualObjSer>());
}

public sealed record Curve(
	Guid Id,
	CurvePt[] Pts
) : IVisualObjSer
{
	public static Curve Empty() => new(
		Guid.NewGuid(),
		System.Array.Empty<CurvePt>()
	);

	public R BoundingBox => this.GetDrawPoints().GetBBox();
	public double DistanceToPoint(Pt pt) => this.GetDrawPoints().DistanceToPoint(pt);

	public override string ToString() => $"{Pts.Length}";
}




[JsonDerivedType(typeof(Curve), typeDiscriminator: "Curve")]
public interface IVisualObjSer : IVisualObj;



static class DocExt
{
	public static Doc AddObject(this Doc doc, IVisualObjSer obj) => doc.ChangeLayer(arr => arr.AddId(obj));

	private static Doc ChangeLayer(this Doc doc, Func<IVisualObjSer[], IVisualObjSer[]> fun) => doc.WithLayers(doc.Layers.ChangeId(doc.Layers[0].Id, layer => layer.WithObjects(fun(layer.Objects))));
	private static Doc WithLayers(this Doc m, Layer[] xs) => m with { Layers = xs };
	private static Layer WithObjects(this Layer m, IVisualObjSer[] xs) => m with { Objects = xs };
}

