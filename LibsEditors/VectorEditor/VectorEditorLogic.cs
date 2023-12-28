using LinqVec;
using LinqVec.Logic;
using ReactiveVars;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.Select_;

namespace VectorEditor;

public static class VectorEditorLogic
{
	public static IUndoerReadOnly<Doc> InitVectorEditor(this VecEditor vecEditor, Doc? initModel, Disp d)
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
}