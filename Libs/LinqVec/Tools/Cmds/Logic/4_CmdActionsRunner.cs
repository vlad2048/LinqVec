using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Structs;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;

static class CmdActionsRunner
{
	public static void Run_Cmd_Actions(
		this IObservable<ICmdEvt> cmdEvt,
		IRoVar<Option<Hotspot>> hotspot,
		IRwVar<bool> isDragging,
		IRoVar<Pt> mouse,
		Action<ToolStateFun> setState,
		Disp d
	)
	{
		Obs.Merge(
				cmdEvt.OfType<ConfirmCmdEvt>().Where(e => e.HotspotCmd is ClickHotspotCmd)
					.Do(
						cmd =>
						{
							isDragging.V = true;
							var stateNextOpt = ((ClickHotspotCmd)cmd.HotspotCmd).ClickAction();
							stateNextOpt.IfSome(setState);
							isDragging.V = false;
						}
					)
					.ToUnit(),
				cmdEvt.OfType<DragStartCmdEvt>()
					.Select(
						cmd =>
						{
							isDragging.V = true;
							//LR.LogThread("           Drag Start_1");
							var stopFun = ((DragHotspotCmd)cmd.HotspotCmd).DragAction(cmd.PtStart, mouse);
							//LR.LogThread("           Drag Start_2");
							return
								Obs.Amb(
										cmdEvt.OfType<CancelCmdEvt>().Select(_ => false),
										cmdEvt.OfType<DragFinishCmdEvt>().Select(_ => true)
									)
									.Take(1)
									.Do(commit =>
									{
										//LR.LogThread("           Drag Stop_1");
										stopFun(commit);
										//LR.LogThread("           Drag Stop_2");
										isDragging.V = false;
									});
						}
					)
					.Switch()
					.Repeat()
					.ToUnit()
			)
			.Subscribe(_ => {}).D(d);
	}
}