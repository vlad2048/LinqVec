using BrightIdeasSoftware;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using ReactiveVars;
using UILib.Utils;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.Select_;

namespace VectorEditor;


public sealed class VectorEditorLogic : EditorLogic<Doc>
{
	public override EditorLogicCaps Caps => EditorLogicCaps.SupportLayoutPane;

	public override IUndoerReadOnly<Doc> Init(VecEditor vecEditor, Doc? initModel, Disp d)
	{
		var env = vecEditor.Env;
		var doc = new Unmod<Doc>(initModel ?? Doc.Empty(), d);

		vecEditor.Init(
			new VecEditorInitNfo(
				doc,
				[
					new CurveTool(env, doc),
					new SelectTool(env, doc),
				]
			)
		);

		env.WhenPaint
			.Subscribe(gfx =>
			{
				var m = doc.VModded;
				foreach (var layer in m.Layers)
				foreach (var obj in layer.Objects)
				{
					switch (obj)
					{
						case Curve curve:
							Painter.PaintCurve(gfx, curve, CurveGfxState.None);
							break;
					}
				}
			}).D(d);

		return doc;
	}

	public override void SetupLayoutPane(TreeListView tree, IObservable<Option<Doc>> doc, Disp d)
	{
		tree.SetNodGeneric<LayoutTreeLogic.DocNode>();
		tree.SetupColumns();
		doc.WhereNone().Subscribe(_ => tree.ClearObjects()).D(d);
		doc.WhereSome()
			.Subscribe(cur =>
			{
				var roots = cur.ToTree();
				tree.SetObjects(roots);
				tree.ExpandAll();
			}).D(d);
	}
}




file static class LayoutTreeLogic
{
	public sealed record DocNode(
		IId Obj
	);

	public static TNod<DocNode>[] ToTree(this Doc doc) =>
		doc.Layers.SelectToArray(layer =>
			Nod.Make(new DocNode(layer),
				layer.Objects.Select(obj => Nod.Make(new DocNode(obj)))
			)
		);

	public static void SetupColumns(this ObjectListView list)
	{
		list.AddTextColumn<TNod<DocNode>>("name", null, nod => nod.V.Obj switch
		{
			Layer => "layer",
			Curve => "curve",
			_ => "unknown"
		});

		list.AddTextColumn<TNod<DocNode>>("info", null, nod => nod.V.Obj switch
		{
			Layer e => $"kids:{e.Objects.Length}",
			Curve e => $"points:{e.Pts.Length}",
			_ => "unknown"
		});

		list.AddTextColumn<TNod<DocNode>>("id", 70, nod => $"{nod.V.Obj.Id}");
	}
}
