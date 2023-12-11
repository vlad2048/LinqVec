using LinqVec;
using LinqVec.Logic;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using LinqVec.Tools.Events.Utils;
using PowRxVar;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using VectorEditor.Tools.Select_;

namespace VectorEditor;

public static class VectorEditorLogic
{
	public static (Model<Doc> , IDisposable) InitVectorEditor(this VecEditor vecEditor, Doc? initModel = null)
	{
		var d = new Disp();

		var env = vecEditor.Env;
		var doc = new Model<Doc>(initModel ?? Doc.Empty).D(d);

		var evt = env.EditorEvt
			.ToGrid(env.Transform)
			.SnapToGrid()
			.RestrictToGrid()
			.TrackMouse(out var mousePos, d)
			.MakeHot(d);

		vecEditor.Init(
			new VecEditorInitNfo(
				doc,
				new ITool[]
				{
					new CurveTool(env, doc),
					new SelectTool(env, doc),
				}
			)
		);

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

		Obs.Merge(
				//doc.WhenChanged.ToUnit(),
				mousePos.ToUnit()
			)
			.Subscribe(_ =>
			{
				env.Invalidate();
			}).D(d);

		return (doc, d);
	}

	private static readonly Brush brush = new SolidBrush(Color.DodgerBlue);
}