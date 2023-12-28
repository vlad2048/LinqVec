using System.Reactive.Linq;
using LinqVec.Tests.ModelTesting.TestSupport;
using LinqVec.Tests.Tools.Cmds.TestSupport;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Events;
using TestLib;

namespace LinqVec.Tests.Tools.Cmds;

class CmdRunnerTests : ModelTestBase
{
	private Evt evt = null!;

	[Test]
	public void _00_Basic()
	{
		// Setup
		// =====
		evt = new Evt(
			Sched.CreateHotObservable<IEvt>([
				Move(1, -3),
				Move(2, 4),
				Move(3, 5),
				LDown(4),
				Move(5, 7),
				Move(6, 10),
				LUp(7),
			]),
			_ => {},
			Obs.Never<Unit>(),
			D
		);

		// Run
		// ===
		var output = StateAddPoint(Unit.Default).Run(evt, () => {}, Sched, D);

		// Observe
		// =======
		var dbgEvt = Subs(
			DbgEvtUtils.Make(output, Curve, Sched)
				.Select(e => e switch {
					RunDbgEvt { Evt: HotspotChangedRunEvt { Hotspot: var hotspot } } => $"[Hotspot] <- {hotspot}",
					CmdDbgEvt { Evt: ConfirmCmdEvt { HotspotCmd.Name: var name } } => $"Cmd[{name}]",
					ModDbgEvt { Evt: SetModEvt { Name: var name } } => $"                      -> mod.set({name})",
					ModDbgEvt { Evt: ApplyModEvt { Name: var name } } => $"                      -> mod.app({name})",
					_ => string.Empty
				})
				.Where(e => e != string.Empty)
		);

		var doc = Subs(Model.Cur);
		var ptrGfx = Subs(Curve.WhenPaintNeeded.Select(_ => Curve.ModGet()));

		// Start
		// =====
		//Sched.Start();
		Sched.AdvanceTo(10.Sec());

		// Log
		// ===
		dbgEvt.LogMessages("DbgEvt");
		doc.LogMessages("Doc");
		ptrGfx.LogMessages("PtrGfx");

		// Assert
		// ======
		/*
		docObs.AssertEq([
			(0, [[]]),
			(2, [[(4, 4)]]),
			(3, [[(5, 5)]]),
		]);
		*/
	}


	ToolStateFun StateAddPoint(Unit _) => _ => new ToolState(
		nameof(StateAddPoint),
		null!,
		[
			Hotspots.Right
				.OnHover(Curve.HoverMod(Mods.AddPoint_Hover(evt.MousePos, D)))
				.Do(_ => [
					Cmd.Drag(
						"AddPoint",
						Curve.DragMod(Mods.AddPoint_Drag(evt.MousePos, D))
					)
				]),
			Hotspots.Left
				.Do(_ => [
				]),
		]
	);
}