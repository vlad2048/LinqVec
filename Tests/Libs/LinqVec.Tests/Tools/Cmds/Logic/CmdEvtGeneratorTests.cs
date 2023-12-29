using System.Reactive;
using Geom;
using LinqVec.Tests.TestSupport;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Events;
using Microsoft.Reactive.Testing;
using ReactiveVars;
using TestLib;

namespace LinqVec.Tests.Tools.Cmds.Logic;

class CmdEvtGeneratorTests
{
	private static readonly DragHotspotCmd cmdDrag = new("Drag", Gesture.Drag, _ => () => {});
	private static readonly ClickHotspotCmd cmdClick = new("Click", Gesture.Click, () => None);
	private static readonly ClickHotspotCmd cmdRightClick = new("RightClick", Gesture.RightClick, () => None);
	private static readonly ClickHotspotCmd cmdDoubleClick = new("DoubleClick", Gesture.DoubleClick, () => None);

	private static IRoVar<HotspotNfoResolved> MkActsRun(IHotspotCmd[] cmds) => Var.MakeConst(new HotspotNfoResolved(
		Hotspot.Empty,
		123,
		cmds,
		false
	));

	[Test]
	public void _0_DragClick()
	{
		var sched = new TestScheduler();
		var actsRun = MkActsRun([cmdDrag, cmdClick]);
		var evt = sched.CreateHotObservable([
			Move(1),

			LDown(2),
			Move(3),
			LUp(4),

			LDown(5),
			LUp(10),

			LDown(11),
			LUp(11.1),

			Move(12),
			LDown(13),
			Move(14),
			Move(15),
			LUp(16)
		]);

		var obs = sched.CreateObserver<ICmdEvt>();
		using var d = new Disp();
		actsRun.ToCmdEvt(evt, sched, d).Subscribe(obs);

		sched.Start();
		obs.LogMessages("ActEvt");

		obs.Messages.AssertEqual([
			OnNext(3, (ICmdEvt)new DragStartCmdEvt(cmdDrag, Pt.Zero)),
			OnNext(4, (ICmdEvt)new ConfirmCmdEvt(cmdDrag, Pt.Zero)),
			OnNext(11.1, (ICmdEvt)new ConfirmCmdEvt(cmdClick, Pt.Zero)),
			OnNext(14, (ICmdEvt)new DragStartCmdEvt(cmdDrag, Pt.Zero)),
			OnNext(16, (ICmdEvt)new ConfirmCmdEvt(cmdDrag, Pt.Zero)),
		]);
	}


	[Test]
	public void _1_ClickDoubleClick()
	{
		var sched = new TestScheduler();
		var actsRun = MkActsRun([cmdClick, cmdDoubleClick]);
		var evt = sched.CreateHotObservable([
			Move(1),
			LDown(2),
			Move(2.1),
			LUp(2.2),

			LDown(3),
			LUp(3.1),

			LDown(5),
			LUp(5.1),
			LDown(5.2),
			LUp(5.3),
		]);

		var obs = sched.CreateObserver<ICmdEvt>();
		using var d = new Disp();
		actsRun.ToCmdEvt(evt, sched, d).Subscribe(obs);

		sched.Start();
		obs.LogMessages("ActEvt");

		var delay = CmdEvtGenerator.ClickDelay.TotalSeconds;

		obs.Messages.AssertEqual([
			OnNext(3.1 + delay, (ICmdEvt)new ConfirmCmdEvt(cmdClick, Pt.Zero)),
			OnNext(5.3, (ICmdEvt)new ConfirmCmdEvt(cmdDoubleClick, Pt.Zero)),
		]);
	}


	[Test]
	public void _2_RightClick()
	{
		var sched = new TestScheduler();
		var actsRun = MkActsRun([cmdRightClick]);
		var evt = sched.CreateHotObservable([
			Move(1),
			LDown(2),
			LUp(2.1),

			RDown(3),
			RUp(5),

			RDown(6),
			RUp(6.1),

			RDown(8),
			Move(8.1),
			RUp(8.2),
		]);

		var obs = sched.CreateObserver<ICmdEvt>();
		using var d = new Disp();
		actsRun.ToCmdEvt(evt, sched, d).Subscribe(obs);

		sched.Start();
		obs.LogMessages("ActEvt");

		obs.Messages.AssertEqual([
			OnNext(6.1, (ICmdEvt)new ConfirmCmdEvt(cmdRightClick, Pt.Zero)),
		]);
	}




	private static Recorded<Notification<IEvt>> Move(double t) => OnNext(t, (IEvt)new MouseMoveEvt(Pt.Zero));
	private static Recorded<Notification<IEvt>> LDown(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Down, MouseBtn.Left, ModKeyState.Empty));
	private static Recorded<Notification<IEvt>> RDown(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Down, MouseBtn.Right, ModKeyState.Empty));
	private static Recorded<Notification<IEvt>> LUp(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Up, MouseBtn.Left, ModKeyState.Empty));
	private static Recorded<Notification<IEvt>> RUp(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Up, MouseBtn.Right, ModKeyState.Empty));
}