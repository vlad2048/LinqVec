using System.Reactive.Linq;
using System.Xml.Linq;
using Geom;
using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
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

	private static Func<T, T> Mk<T>(Func<T, T> f) => f;

	/*public void Run(Disp d)
	{
		var evt = c.Env.GetEvtForTool(this, true, d);
		var curve = c.Doc.Scope(Curve.Empty(), (e, _) => e, CurveFuns.Create_SetFun, CurveFuns.Create_ValidFun).D(d);

		Action<bool>? action = null;

		evt.WhenKeyDown(Keys.D8).Subscribe(_ =>
		{
			L.WriteLine("Drag -> Start");
			Func<Pt, Pt, Curve, Curve> fun = (ptStart, ptEnd, curveV) => curveV with
			{
				Pts = curveV.Pts.AddArr(CurvePt.Make(ptEnd, ptEnd))
			};
			(var source, action) = evt.MousePos
				.Select(ptEndOpt => ptEndOpt.Match(
					ptEndV => Mk<Curve>(ptrV => fun(Pt.Zero, ptEndV, ptrV)),
					() => Mk<Curve>(ptrV => ptrV)
				))
				.ToVar()
				.TerminateWithAction();
			var sourceHot = source.MakeHot(d);
			//sourceHot.Materialize().Subscribe(e => L.WriteLine($"{e}")).D(d);
			var mod = new Mod<Curve>("Test", sourceHot);
			curve.SetMod(mod);
		}).D(d);
		evt.WhenKeyDown(Keys.D9).Subscribe(_ =>
		{
			L.WriteLine("Drag -> SendComplete");
			action?.Invoke(true);
		}).D(d);
		evt.WhenKeyDown(Keys.D0).Subscribe(_ =>
		{
			L.WriteLine("Drag -> SendError");
			action?.Invoke(true);
		}).D(d);

	}*/



	public void Run(Disp d)
	{
		var evt = c.Env.GetEvtForTool(this, true, d);
		var curve = c.Doc.Scope(Curve.Empty(), (e, _) => e, CurveFuns.Create_SetFun, CurveFuns.Create_ValidFun).D(d);

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
				//Hotspots.CurvePoint(curve.V.V, false)
				//	.OnHover(
				//		Cmd.EmptyHoverAction
				//			.UpdateGizmoTemp(gizmoApply, _ => CurveGfxState.Edit)
				//	)
				//	.Do(pointId => [
				//		Cmd.Drag(
				//			Cmds.MovePoint,
				//			curve.ModSetDrag("Curve_MovePoint", (ptStart, ptEnd, curveV) => curveV.MovePoint(pointId, ptEnd))
				//				.UpdateGizmoTemp(gizmoApply, _ => CurveGfxState.Edit)
				//		)
				//	]),

				//.. DoesSelectionContainExactlyOneCurve(c.State, c.Doc, out var curveId)
				//? new[] {
				//	Cmd.Drag(
				//		Cmds.ContinueCurve,
				//
				//	)
				//}
				//: [],



				Hotspots.AnywhereNeg
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
