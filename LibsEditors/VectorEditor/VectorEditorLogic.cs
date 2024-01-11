using System.Reactive.Linq;
using BrightIdeasSoftware;
using LinqVec;
using LinqVec.Interfaces;
using LinqVec.Structs;
using LinqVec.Tools;
using LinqVec.Utils.Json;
using PowBasics.CollectionsExt;
using PowBasics.Json_;
using PtrLib;
using ReactiveVars;
using Geom;
using UILib.Utils;
using VectorEditor._Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.CurveEdit_;
using VectorEditor.Tools.Select_;

namespace VectorEditor;

public sealed class VectorEditorLogicMaker : EditorLogicMaker
{
	public override EditorLogicCaps Caps => EditorLogicCaps.SupportLayoutPane;
	public override VectorEditorLogic Make(Option<string> filename, ToolEnv env, Disp d) => new(filename, env, d);
	private VectorEditorLogicMaker() {}
	public static readonly VectorEditorLogicMaker Instance = new();
}

sealed record Ctx(
	IPtr<Doc> Doc,
	IRwVar<EditorState> State,
	ToolEnv Env
);

public sealed class VectorEditorLogic : EditorLogic
{
	public override IDocHolder DocHolder { get; }
	public override ITool[] Tools { get; }

	public IPtr<Doc> Doc { get; }

	public VectorEditorLogic(Option<string> filename, ToolEnv env, Disp d)
	{
		var docInit = filename.Match(
			VecJsoner.Vec.Load<Doc>,
			_Model.Doc.Empty
		);
		Doc = Ptr.Make(docInit, d);
		DocHolder = LinqVec.Structs.DocHolder.Make(Doc);
		var state = EditorState.Empty.Make(d);

		var ctx = new Ctx(Doc, state, env);
		Tools = [
			new SelectTool(ctx),
			new CurveEditTool(ctx),
			new CurveTool(ctx),
		];


		state.Subscribe(_ => env.Invalidate()).D(d);

		G.Cfg.RunWhen(e => e.Log.EditorState, d, () => state.LogD("State"));


		env.WhenPaint
			.Subscribe(gfx =>
			{
				foreach (var layer in Doc.VGfx.V.Layers)
				foreach (var obj in layer.Objects)
				{
					switch (obj)
					{
						case Curve curve:
							Painter.DrawCurve(gfx, curve, false);
							break;
					}
				}

				var bboxOpt =
					Doc.VGfx.V.GetObjects(state.V.Selection)
						.Select(e => e.BoundingBox)
						.Union();
				Painter.PaintSelectRectangle(gfx, bboxOpt);
			}).D(d);
	}


	public override void Save(string filename) => VecJsoner.Vec.Save(filename, Doc.V);



	public override void SetupLayoutPane(TreeListView tree, Disp d)
	{
		tree.SetNodGeneric<LayoutTreeLogic.DocNode>();
		tree.SetupColumns();
		Doc.V.WhenOuter
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
