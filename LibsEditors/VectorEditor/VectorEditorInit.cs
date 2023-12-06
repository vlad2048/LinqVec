using System.Reactive;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Tools;
using LinqVec.Tools.Events;
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

		var evt = env.EditorEvt
			.ToGrid(env.Transform)
			.SnapToGrid(env.Transform)
			.TrackPos(out var mousePos, d)
			.MakeHot(d);

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
				var m = mm.GetGfxModel(mousePos);
				foreach (var layer in m.Layers)
				{
					foreach (var obj in layer.Objects)
					{
						switch (obj)
						{
							case CurveModel curve:
								CurveModelPainter.Draw(gfx, curve);
								break;
						}
					}
				}
			}).D(d);

		Obs.Merge(
				mm.WhenChanged,
				mousePos.ToUnit()
			)
			.Subscribe(_ =>
			{
				env.Invalidate();
			}).D(d);

		return (mm, d);
	}
}