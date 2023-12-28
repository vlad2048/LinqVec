using LinqVec.Utils;
using PowBasics.CollectionsExt;

namespace LinqVec.Tests.ModelTesting.TestSupport;


interface IObj : IId;

sealed record Doc(
	Layer[] Layers,
	Guid ActiveLayer
) : IDoc
{
	public static Doc Empty() => new([new Layer(Guid.Empty, [])], Guid.Empty);
	public override string ToString() => "[" + Layers.SelectMany(e => e.Objects).JoinText("; ") + "]";
	public string GetUndoRedoStr() => ToString();
}


sealed record Layer(
	Guid Id,
	IObj[] Objects
) : IId
{
	public static Layer Empty() => new(Guid.NewGuid(), []);
}

sealed record CurvePt(int Start, int End);

sealed record Curve(
	Guid Id,
	CurvePt[] Pts
) : IObj
{
	public static Curve Empty() => new(Guid.NewGuid(), []);
	public override string ToString() => $"Curve({Pts.Select(e => $"({e.Start},{e.End})").JoinText("; ")})";
}



/*
static class Entities
{
	public static IPtr<Doc, Curve> CurveCreate(this Model<Doc> model, Disp d) => new PtrCreate<Doc, Curve, Guid>(model, Curve, d);


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