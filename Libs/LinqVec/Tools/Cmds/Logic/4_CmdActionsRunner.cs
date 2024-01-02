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
		IRwVar<bool> isHotspotFrozen,
		IRoVar<Pt> mouse,
		Action<ToolStateFun> setState,
		Disp d
	)
	{
		void Freeze(bool enable)
		{
			isHotspotFrozen.V = enable;
		}

		Obs.Merge(
				cmdEvt.OfType<ConfirmCmdEvt>().Where(e => e.HotspotCmd is ClickHotspotCmd)
					.Do(
						cmd =>
						{
							Freeze(true);
							var stateNextOpt = ((ClickHotspotCmd)cmd.HotspotCmd).ClickAction();
							stateNextOpt.IfSome(setState);
							Freeze(false);
						}
					)
					.ToUnit(),
				cmdEvt.OfType<DragStartCmdEvt>()
					.Select(
						cmd =>
						{
							Freeze(true);
							var stopFun = ((DragHotspotCmd)cmd.HotspotCmd).DragAction(cmd.PtStart, mouse);
							return
								Obs.Amb(
										cmdEvt.OfType<CancelCmdEvt>().Select(_ => false),
										cmdEvt.OfType<DragFinishCmdEvt>().Select(_ => true)
									)
									.Take(1)
									.Do(commit =>
									{
										stopFun(commit);
										Freeze(false);
									});
						}
					)
					.Switch()
					.Repeat()
					.ToUnit()
			)
			.Subscribe(_ => {}).D(d);
	}


	/*public static void Run_Cmd_Actions(
		this IObservable<ICmdEvt> cmdEvt,
		IRoVar<Option<Hotspot>> hotspot,
		IRwVar<bool> hotspotTrackingEnabled,
		IRoVar<Pt> mouse,
		Action<ToolStateFun> setState,
		DISP d
	)
	{
		Obs.Merge<IAct>(
			cmdEvt.OfType<ConfirmCmdEvt>().Where(e => e.HotspotCmd is ClickHotspotCmd)
				.Select(
					cmd => new ClickAction(
						() =>
						{
							var stateNextOpt = ((ClickHotspotCmd)cmd.HotspotCmd).ClickAction();
							stateNextOpt.IfSome(setState);
						},
						cmd.HotspotCmd.Name,
						cmd.HotspotCmd.Gesture
					)
				),
			cmdEvt.OfType<DragStartCmdEvt>()
				.Select(
					cmd =>
					{
						var stopFun = ((DragHotspotCmd)cmd.HotspotCmd).DragAction(cmd.PtStart, mouse);
						return
							Obs.Amb(
								cmdEvt.OfType<CancelCmdEvt>().Select(_ => false),
								cmdEvt.OfType<ConfirmCmdEvt>().Select(_ => true)
							)
							.Take(1)
							.Do(stopFun);
					}
				)
		)
	}*/


	/*private interface IAct
	{
		Action Run { get; }
	}
	private sealed record ClickAction(Action Run, string CmdName, Gesture CmdGesture) : IAct
	{
		public override string ToString() => $"ClickAction({CmdName}, {CmdGesture})";
	}
	private sealed record DragActionStart(Action Run, string CmdName, Gesture CmdGesture) : IAct
	{
		public override string ToString() => $"DragAction({CmdName}, {CmdGesture})";
	}
	private sealed record DragActionStop(Action Run, string CmdName, Gesture CmdGesture, bool Commit) : IAct
	{
		public override string ToString() => $"DragAction({CmdName}, {CmdGesture}) - Stop({Commit})";
	}*/
}