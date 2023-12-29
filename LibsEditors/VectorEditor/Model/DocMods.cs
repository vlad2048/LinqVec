using System.Reactive.Linq;
using Geom;
using LinqVec;
using LinqVec.Logic.Structs;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using ReactiveVars;
using VectorEditor.Model.Structs;

namespace VectorEditor.Model;

static class DocMods
{
	public static Func<Pt, Mod<Doc>> MoveSelection(IRoVar<Option<Pt>> mouse, Guid[] selObjIds, Disp d) =>
		startPt =>
			new(
				nameof(MoveSelection),
				true,
				mouse
					.WhereSome()
					.Select(m => Mk(doc => MoveSelection(doc, selObjIds, m - startPt)))
					.ToVar(d)
			);

	private static Func<Doc, Doc> Mk(Func<Doc, Doc> f) => f;

	private static Doc MoveSelection(Doc doc, Guid[] selObjIds, Pt delta) =>
		selObjIds.Aggregate(
			doc,
			(acc, id) => MoveSelection(acc, id, delta)
		);

	private static Doc MoveSelection(Doc doc, Guid selObjId, Pt delta) =>
		doc with {
			Layers = doc.Layers
				.SelectToArray(
					layer => layer with {
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
		return curve with {
			Pts = curve.Pts.SelectToArray(e => Move(e, delta))
		};
	}

	private static CurvePt Move(CurvePt p, Pt delta) => new(
		p.P + delta,
		p.HLeft + delta,
		p.HRight + delta
	);
}