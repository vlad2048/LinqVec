using System.Reactive.Linq;
using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;
using VectorEditor._Model;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(Keys shortcut) : ITool<Doc>
{
	public string Name => "C";
	public Bitmap? Icon => Resource.toolicon_CurveCreate;
	public Keys Shortcut => shortcut;

	private static class States
	{
		public const string Neutral = nameof(Neutral);
	}
	private static class Cmds
	{
		public const string MovePoint = nameof(MovePoint);
		public const string AddPoint = nameof(AddPoint);
	}

	public Disp Run(ToolEnv<Doc> Env, ToolActions toolActions)
	{
		var d = MkD();
		var doc = Env.Doc;
		var evt = Env.GetEvtForTool(this, true, d);

		var curve = doc.Create(Curve.Empty(), CurveFuns.Create_SetFun, CurveFuns.Create_ValidFun, d);

		var gizmo = CurveGfxState.AddPoint;
		Action<Func<CurveGfxState, CurveGfxState>> gizmoApply = f => gizmo = f(gizmo);
		gizmoApply = gizmoApply.Log("CurveTool");


		evt.WhenKeyDown(Keys.Enter)
			.ObserveOnUI()
			.Subscribe(_ =>
			{
				curve.Commit();
				toolActions.Reset();
			}).D(d);

		ToolStateFun ModeNeutral(Unit _) => _ => new ToolState(
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


		var cmdOutput =
			ModeNeutral(Unit.Default)
				.Run(evt, Env.Invalidate, d);


		/*cmdOutput
			.WhenRunEvt
			.Select(e => e switch
			{
				//DragStartRunEvt {Cmd: Cmds.AddPoint} => CurveGfxState.DragHandle,
				//HotspotChangedRunEvt {Hotspot: Hotspots.CurvePointId} => CurveGfxState.Edit,
				_ => CurveGfxState.AddPoint
			})
			.Subscribe(e => gizmo = e).D(d);*/



		Env.WhenPaint.Subscribe(gfx =>
		{
			Painter.PaintCurve(gfx, curve.VModded, gizmo);
		}).D(d);

		return d;
	}


	/*
	private const int HotspotKey = 0x979b32;
	private const int HotspotVal = 0xf2f77d;
	private const int ModKey = 0x451094;
	private const int ModVal = 0x8548e0;
	private const int CmdKey = 0x991a1a;
	private const int CmdVal = 0xfa5555;

	private void Log(CmdOutput cmdOutput, IPtr<Doc, Curve> curve, Disp d)
	{
		G.Cfg.RunWhen(e => e.Log.LogCmd.DbgEvt, d, [
			() => DbgEvtUtils.Make(cmdOutput, curve, Rx.Sched)
				.Subscribe(e =>
				{
					switch (e)
					{
						case RunDbgEvt { Evt: HotspotChangedRunEvt { Hotspot: var hotspot } }:
							L.Write("[Hotspot] <- ", HotspotKey);
							L.WriteLine(hotspot, HotspotVal);
							break;

						case CmdDbgEvt { Evt: ConfirmCmdEvt { HotspotCmd.Name: var name } }:
							L.Write("Cmd[", CmdKey);
							L.Write(name, CmdVal);
							L.WriteLine("]", CmdKey);
							break;

						case ModDbgEvt { Evt: SetModEvt { Name: var name } }:
							L.Write("                      -> mod.set(", ModKey);
							L.Write(name, ModVal);
							L.WriteLine(")", ModKey);
							break;

						case ModDbgEvt { Evt: ApplyModEvt { Name: var name } }:
							L.Write("                      -> mod.app(", ModKey);
							L.Write(name, ModVal);
							L.WriteLine(")", ModKey);
							break;
					}

					//var str = e switch {
					//	RunDbgEvt { Evt: HotspotChangedRunEvt { Hotspot: var hotspot } } => $"[Hotspot] <- {hotspot}",
					//	CmdDbgEvt { Evt: ConfirmCmdEvt { HotspotCmd.Name: var name } } => $"Cmd[{name}]",
					//	ModDbgEvt { Evt: SetModEvt { Name: var name } } => $"                      -> mod.set({name})",
					//	ModDbgEvt { Evt: ApplyModEvt { Name: var name } } => $"                      -> mod.app({name})",
					//	_ => string.Empty
					//};
					//L.WriteLine(str);
				})
		]);
	}*/




		/*
		Doc.EnableRedrawOnMouseMove(d);
		
		var curve = new MemMouseModder<Curve>(Curve.Empty()).D(d);
		toolActions.SetUndoer(curve.Undoer);
		var gfxState = CurveGfxState.None;

		evt.WhenKeyDown(Keys.Escape).Subscribe(_ => toolActions.Reset()).D(d);
		evt.WhenKeyDown(Keys.Enter).Subscribe(_ =>
		{
			if (curve.Get().Pts.Length > 0)
				Doc.V = Doc.V.AddObject(curve.Get());
			toolActions.Reset();
		}).D(d);



		ToolStateFun ModeNeutral(Unit _) => _ => new ToolState(
			States.Neutral,
			CBase.Cursors.Pen,
			[
				Hotspots.CurvePointButLast(curve)
					.Do(pointId => [
						Cmd.Drag(
							Cmds.MovePoint,
							curve,
							CurveMods.MovePoint(pointId)
						)
					]),
				Hotspots.Anywhere
					.Do(_ => [
						Cmd.Drag(
							Cmds.AddPoint,
							curve,
							CurveMods.AddPoint(Unit.Default)
						)
					]),
			]
		);

		ModeNeutral(Unit.Default)
			.Run(evt, d)
			.Subscribe(e =>
			{
				switch (e)
				{
					case HotspotHoverRunEvt { Hotspot: "Anywhere", On: true }:
						curve.ModSet(CurveMods.AddPoint());
						gfxState = CurveGfxState.AddPoint;
						break;
					case HotspotHoverRunEvt { Hotspot: "Anywhere", On: false }:
						curve.ModClear();
						gfxState = CurveGfxState.None;
						break;

					case DragStartRunEvt { Cmd: Cmds.AddPoint }:
						gfxState = CurveGfxState.DragHandle;
						break;
					case ConfirmRunEvt { Cmd: Cmds.AddPoint }:
						curve.ModSet(CurveMods.AddPoint());
						gfxState = CurveGfxState.AddPoint;
						break;

					case DragStartRunEvt { Cmd: Cmds.MovePoint }:
						gfxState = CurveGfxState.Edit;
						break;
				}
			}).D(d);


		Env.WhenPaint.Subscribe(gfx =>
		{
			CurvePainter.Draw(gfx, curve.GetModded(evt.MousePos.V), gfxState);
		}).D(d);*/
}
