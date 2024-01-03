using System.Reactive.Linq;
using System.Xml.Linq;
using Geom;
using LinqVec;
using LinqVec.Logging;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Cmds.Utils;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using PtrLib;
using ReactiveVars;
using VectorEditor._Model;
using VectorEditor._Model.Structs;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(Ctx c) : ITool
{
	public ToolNfo Nfo { get; } = new(
		"C",
		Resource.toolicon_CurveCreate,
		Keys.F3
	);

	private static class States
	{
		public const string Neutral = nameof(Neutral);
	}
	private static class Cmds
	{
		public const string MovePoint = nameof(MovePoint);
		public const string AddPoint = nameof(AddPoint);
		public const string ContinueCurve = nameof(ContinueCurve);
	}

	private static bool DoesSelectionContainExactlyOneCurve(IRoVar<EditorState> state, IPtr<Doc> doc, out Guid curveId)
	{
		curveId = Guid.Empty;
		var sel = state.V.Selection;
		if (sel.Length != 1) return false;
		if (doc.V.V.GetObject<Curve>(sel[0]).IsSome)
		{
			curveId = sel[0];
			return true;
		}
		return false;
	}


	public void Run(Disp d)
	{
		var evt = c.Env.GetEvtForTool(this, true, d);
		var curve = c.Doc.Scope(Curve.Empty(), (e, _) => e, CurveFuns.Create_SetFun, CurveFuns.Create_ValidFun).D(d);
		LogCategories.Setup_ModEvt_Logging(curve.WhenModEvt.Select(e => e.Conv()), Rx.Sched, d);

		var gizmo = CurveGfxState.AddPoint;
		Action<Func<CurveGfxState, CurveGfxState>> gizmoApply = f => gizmo = f(gizmo);
		//gizmoApply = gizmoApply.Log("CurveTool");


		evt.WhenKeyDown(Keys.Enter)
			.ObserveOnUI()
			.Subscribe(_ =>
			{
				curve.Commit();
				c.Env.ToolReset();
			}).D(d);

		ToolStateFun ModeNeutral() => _ => new ToolState(
			States.Neutral,
			CBase.Cursors.Pen,
			[
				Hotspots.CurvePoint(curve.V.V, false)
					.OnHover(
						Cmd.EmptyHoverAction
							.UpdateGizmoTemp(gizmoApply, _ => CurveGfxState.Edit)
					)
					.Do(pointId => [
						Cmd.Drag(
							Cmds.MovePoint,
							curve.ModSetDrag("Curve_MovePoint", (ptStart, ptEnd, curveV) => curveV.MovePoint(pointId, ptEnd))
								.UpdateGizmoTemp(gizmoApply, _ => CurveGfxState.Edit)
						)
					]),

				//.. DoesSelectionContainExactlyOneCurve(c.State, c.Doc, out var curveId)
				//? new[] {
				//	Cmd.Drag(
				//		Cmds.ContinueCurve,
				//
				//	)
				//}
				//: [],



				Hotspots.Anywhere
					.OnHover(
						curve.ModSetHover("Curve_AddPoint_Hover", (pt, curveV) => curveV.AddPoint(pt, pt))
							.UpdateGizmo(gizmoApply, _ => CurveGfxState.AddPoint)
					)
					.Do(_ => [
						Cmd.Drag(
							Cmds.AddPoint,
							curve.ModSetDrag("Curve_AddPoint", (ptStart, ptEnd, curveV) => curveV.AddPoint(ptStart, ptEnd))
								.UpdateGizmoTemp(gizmoApply, _ => CurveGfxState.DragHandle)
						),
					]),
			]
		);


		ModeNeutral()
			.Run(evt, c.Env.Invalidate, d);



		c.Env.WhenPaint.Subscribe(gfx =>
		{
			Painter.PaintCurve(gfx, curve.VGfx.V, gizmo);
		}).D(d);
	}
}
