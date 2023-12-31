﻿using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;
using VectorEditor._Model;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(Ctx ctx) : ITool
{
	public ToolNfo Nfo { get; } = new(
		"C",
		Resource.toolicon_CurveCreate,
		Keys.P
	);

	private static class States
	{
		public const string Neutral = nameof(Neutral);
	}
	private static class Cmds
	{
		public const string MovePoint = nameof(MovePoint);
		public const string AddPoint = nameof(AddPoint);
	}

	public void Run(Disp d)
	{
		var doc = ctx.Doc;
		var evt = ctx.Env.GetEvtForTool(this, true, d);

		var curve = doc.Create(Curve.Empty(), CurveFuns.Create_SetFun, CurveFuns.Create_ValidFun, d);

		var gizmo = CurveGfxState.AddPoint;
		Action<Func<CurveGfxState, CurveGfxState>> gizmoApply = f => gizmo = f(gizmo);
		gizmoApply = gizmoApply.Log("CurveTool");


		evt.WhenKeyDown(Keys.Enter)
			.ObserveOnUI()
			.Subscribe(_ =>
			{
				curve.Commit();
				ctx.Env.ToolReset();
			}).D(d);

		ToolStateFun ModeNeutral() => _ => new ToolState(
			States.Neutral,
			CBase.Cursors.Pen,
			[
				Hotspots.CurvePoint(curve.V, false)
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
			.Run(evt, ctx.Env.Invalidate, d);



		ctx.Env.WhenPaint.Subscribe(gfx =>
		{
			Painter.PaintCurve(gfx, curve.VModded, gizmo);
		}).D(d);
	}
}
