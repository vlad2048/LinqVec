using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Structs;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;

static class CmdActionsRunner
{
	public static IRoVar<Option<string>> Run_Cmd_Actions(
		this IObservable<ICmdEvt> cmdEvt,
		IRoVar<Pt> mouse,
		Action stateRecalc,
		Disp d
	)
	{
		var dragAction = Option<string>.None.Make(d);

		Obs.Merge(
				cmdEvt.OfType<ConfirmCmdEvt>()
					.Do(
						cmd =>
						{
							cmd.HotspotCmd.ClickAction();
							stateRecalc();
						}
					)
					.ToUnit(),
				cmdEvt.OfType<DragStartCmdEvt>()
					.Select(
						cmd =>
						{
							//LR.LogThread("           Drag Start_1");
							var stopFun = cmd.HotspotCmd.DragAction(cmd.PtStart, mouse);
							dragAction.V = cmd.HotspotCmd.Name;
							//LR.LogThread("           Drag Start_2");
							return
								Obs.Amb(
										cmdEvt.OfType<CancelCmdEvt>().Select(_ => false),
										cmdEvt.OfType<DragFinishCmdEvt>().Select(_ => true)
									)
									.Take(1)
									.Do(commit =>
									{
										dragAction.V = None;
										//LR.LogThread("           Drag Stop_1");
										stopFun(commit);
										//LR.LogThread("           Drag Stop_2");
										stateRecalc();
									});
						}
					)
					.Switch()
					.Repeat()
					.ToUnit()
			)
			.Subscribe(_ => {}).D(d);

		return dragAction;
	}
}