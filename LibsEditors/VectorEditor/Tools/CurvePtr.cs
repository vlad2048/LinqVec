/*
using Geom;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Utils;
using ReactiveVars;
using VectorEditor.Model;

namespace VectorEditor.Tools;

sealed class CurvePtr : IPtr
{
	private static readonly MouseMod<Curve> identity = (o, _) => o; 
	
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly Model<Doc> doc;
	private readonly Undoer<Curve> undoer;
	private readonly Guid layerId;
	private readonly Guid curveId;
	private MouseMod<Curve> mod = identity;

	public Curve V => (Curve)doc.Cur.Layers.Get(layerId).Objects.Get(curveId);

	public Curve ModGet(Option<Pt> mousePos) => mousePos.Map(m => mod(V, m)).IfNone(V);
	public void ModSet(MouseMod<Curve> modFun) => mod = modFun;
	public void ModClear() => mod = identity;
	public void ModApply(Pt mousePos) { undoer.V = ModGet(mousePos); mod = identity; }

	// IPtr
	// ====
	public Guid Id => curveId;
	public bool CustomDraw { get; }
	public bool IsStillValid =>
		doc.Cur.Layers.Any(e => e.Id == layerId) &&
		doc.Cur.Layers.Get(layerId).Objects.Any(e => e.Id == curveId) &&
		doc.Cur.Layers.Get(layerId).Objects.Get(curveId) is Curve;

	private CurvePtr(Model<Doc> doc, Guid? create, (Guid, Guid)? edit, bool customDraw)
	{
		this.doc = doc;
		CustomDraw = customDraw;
		if (create.HasValue)
		{
			var curve = Curve.Empty();
			(layerId, curveId) = (doc.Cur.Layers[0].Id, curve.Id);
			doc.Cur = doc.Cur.ChangeLayer(layerId, objs => objs.Add(curve));
		}
		else
		{
			(layerId, curveId) = (edit!.Value.Item1, edit.Value.Item2);
		}

		undoer = new Undoer<Curve>(V).D(d);
		doc.SetPtr(this);
	}

	public static CurvePtr Create(Model<Doc> doc, Guid layerId, bool customDraw) => new(doc, layerId, null, customDraw);
	public static CurvePtr Edit(Model<Doc> doc, Guid layerId, Guid curveId, bool customDraw) => new(doc, null, (layerId, curveId), customDraw);
}
*/