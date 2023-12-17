using LinqVec;
using LinqVec.Logic;
using PowRxVar;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.Select_;

namespace VectorEditor;

public static class VectorEditorLogic
{
	public static Model<Doc> InitVectorEditor(this VecEditor vecEditor, Doc? initModel, Disp d)
	{
		var env = vecEditor.Env;
		var doc = new Model<Doc>(initModel ?? Doc.Empty, env.EditorEvt).D(d);

		vecEditor.Init(
			new VecEditorInitNfo(
				doc,
				[
					new CurveTool(env, doc),
					new SelectTool(env, doc),
				]
			)
		);

		doc.WhenPaintNeeded.Subscribe(_ => env.Invalidate()).D(d);

		env.WhenPaint
			.Subscribe(gfx =>
			{
				var m = doc.V;
				foreach (var layer in m.Layers)
				foreach (var obj in layer.Objects)
				{
					switch (obj)
					{
						case Curve curve:
							CurvePainter.Draw(gfx, curve, CurveGfxState.None);
							break;
					}
				}
			}).D(d);

		return doc;
	}
}