using System.Reactive;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Tools;
using PowRxVar;
using PowRxVar.Utils;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.Curve_.Utils;

namespace VectorEditor;

public static class VectorEditorInit
{
	public static (ModelMan<DocModel> , IDisposable) InitVectorEditor(this VecEditor vecEditor)
	{
		var d = new Disp();

		var env = vecEditor.Env;
		var (requireToolReset, whenToolResetRequired) = RxEventMaker.Make<Unit>().D(d);
		var mm = new ModelMan<DocModel>(
			DocModel.Empty,
			env.EditorEvt,
			() => requireToolReset(Unit.Default)
		).D(d);

		vecEditor.Init(
			new VecEditorInitNfo(
				whenToolResetRequired,
				new ITool[]
				{
					new CurveTool(env, mm)
				}
			)
		);

		env.WhenPaint
			.Subscribe(gfx =>
			{
				foreach (var curve in mm.V.Curves)
				{
					if (mm.IsEdited(curve)) continue;
					CurveModelPainter.Draw(gfx, curve);
				}
			}).D(d);

		mm.WhenChanged.Subscribe(_ => env.Invalidate()).D(d);

		return (mm, d);
	}
}