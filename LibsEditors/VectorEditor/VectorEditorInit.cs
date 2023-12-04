using LinqVec;
using LinqVec.Logic;
using LinqVec.Tools;
using PowRxVar;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.Curve_.Utils;

namespace VectorEditor;

public static class VectorEditorInit
{
	public static (ModelMan<DocModel> , IDisposable) InitVectorEditor(this VecEditor vecEditor)
	{
		var d = new Disp();

		var modelMan = new ModelMan<DocModel>(
			DocModel.Empty,
			vecEditor.Env.EditorEvt,
			vecEditor.Env.SetNoneTool
		).D(d);

		vecEditor.InitTools(
			new NoneTool(),
			new CurveTool(modelMan)
		);

		vecEditor.Env.WhenPaint
			.Subscribe(gfx =>
			{
				foreach (var curve in modelMan.V.Curves)
				{
					if (modelMan.IsEdited(curve)) continue;
					CurveModelPainter.Draw(gfx, curve);
				}
			}).D(d);

		modelMan.WhenChanged.Subscribe(_ => vecEditor.Env.Invalidate()).D(d);

		return (modelMan, d);
	}
}