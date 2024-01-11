using System.Reactive.Linq;
using LinqVec;
using LinqVec.Logging;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Cmds.Utils;
using PtrLib;
using ReactiveVars;
using VectorEditor._Model;

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
		public const string CloseCurve = nameof(CloseCurve);
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
		c.Env.LogTicker.Log(curve.WhenModEvt.RenderMod(), d);

		curve.V.Where(e => e.Closed)
			.Subscribe(_ =>
			{
				curve.Commit();
				c.Env.ToolReset();
			}).D(d);

		var stateFun = () => new ToolState(
			States.Neutral,
			CBase.Cursors.Pen,
		[
			Hotspots.CurvePoint(curve.VGfx, curve.V.V.CanClose() ? CurvePointType.Middle | CurvePointType.Last : CurvePointType.All)
				.Do(pointId => [
					Cmd.Drag(
						Cmds.MovePoint,
						curve.ModSetDrag("Curve_MovePoint", (ptStart, ptEnd, curveV) => curveV.MovePoint(pointId, ptEnd))
					)
				]),

			.. curve.V.V.CanClose()
				? new[]
				{
					Hotspots.CurvePoint(curve.VGfx, CurvePointType.First)
						.Do(pointId => [
							Cmd.Drag(
								Cmds.CloseCurve,
								curve.ModSetDrag("Curve_Close", (ptStart, ptEnd, curveV) => curveV.CloseCurve(ptEnd))
							)
						])
				}
			: [],

			Hotspots.Anywhere
				.OnHover((p, gfx) => Painter.DrawHoverSeg(gfx, curve.VGfx.V, p))
				.Do(_ => [
					Cmd.Drag(
						Cmds.AddPoint,
						curve.ModSetDrag("Curve_AddPoint", (ptStart, ptEnd, curveV) => curveV.AddPoint(ptStart, ptEnd))
					)
				]),
		]
		);

		var cmdOutput = stateFun.Run(evt, c.Env.LogTicker, d);


		cmdOutput.PaintActionMay.Subscribe(_ => c.Env.Invalidate()).D(d);

		c.Env.WhenPaint.Subscribe(gfx =>
		{
			cmdOutput.PaintActionMay.V(gfx);

			var isAddingPoint = cmdOutput.DragAction.V == Cmds.AddPoint;
			Painter.DrawCurve(gfx, curve.VGfx.V, isAddingPoint);
		}).D(d);
	}
}
