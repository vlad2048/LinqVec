using System.Reactive;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using PowRxVar;
using PowRxVar.Utils;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.Select_;

namespace VectorEditor;

public static class VectorEditorLogic
{
	public static (ModelMan<DocModel> , IDisposable) InitVectorEditor(this VecEditor vecEditor, DocModel? initModel = null)
	{
		var d = new Disp();

		var env = vecEditor.Env;
		var mm = new ModelMan<DocModel>(
			initModel ?? DocModel.Empty,
			env.EditorEvt
		).D(d);

		var evt = env.EditorEvt
			.ToGrid(env.Transform)
			.SnapToGrid(env.Transform)
			.TrackPos(out var mousePos, d)
			.MakeHot(d);

		vecEditor.Init(
			new VecEditorInitNfo(
				new ITool[]
				{
					new CurveTool(env, mm),
					new SelectTool(env, mm),
				}
			)
		);

		env.WhenPaint
			.Subscribe(gfx =>
			{
				var m = mm.GetGfxModel(mousePos);
				foreach (var layer in m.Layers)
				foreach (var obj in layer.Objects)
				{
					if (mm.IsTracked(obj)) continue;
					switch (obj)
					{
						case CurveModel curve:
							CurvePainter.Draw(gfx, curve, CurveGfxState.None);
							break;
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