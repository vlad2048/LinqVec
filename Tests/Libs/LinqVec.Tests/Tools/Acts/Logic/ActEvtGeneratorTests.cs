using System.Reactive;
using Geom;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Acts.Logic;
using LinqVec.Tools.Events;
using Microsoft.Reactive.Testing;
using ReactiveVars;
using TestLib;

namespace LinqVec.Tests.Tools.Acts.Logic;

class ActEvtGeneratorTests
{
	private static readonly HotspotAct actDrag = new("Drag", Gesture.Drag, HotspotActActions.Empty);
	private static readonly HotspotAct actClick = new("Click", Gesture.Click, HotspotActActions.Empty);
	private static readonly HotspotAct actRightClick = new("RightClick", Gesture.RightClick, HotspotActActions.Empty);
	private static readonly HotspotAct actDoubleClick = new("DoubleClick", Gesture.DoubleClick, HotspotActActions.Empty);

	private static IRoVar<HotspotActsRun> MkActsRun(HotspotAct[] acts) => Var.MakeConst(new HotspotActsRun(
		Hotspot.Empty,
		123,
		acts
	));

	[Test]
	public void _0_DragClick()
	{
		var sched = new TestScheduler();
		var actsRun = MkActsRun([actDrag, actClick]);
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

		var obs = sched.CreateObserver<IActEvt>();
		using var d = new Disp();
		actsRun.ToActEvt(evt, sched, d).Subscribe(obs);

		sched.Start();
		obs.LogMessages("ActEvt");

		obs.Messages.AssertEqual([
			OnNext(3, (IActEvt)new DragStartActEvt(actDrag, Pt.Zero)),
			OnNext(4, (IActEvt)new ConfirmActEvt(actDrag, Pt.Zero)),
			OnNext(11.1, (IActEvt)new ConfirmActEvt(actClick, Pt.Zero)),
			OnNext(14, (IActEvt)new DragStartActEvt(actDrag, Pt.Zero)),
			OnNext(16, (IActEvt)new ConfirmActEvt(actDrag, Pt.Zero)),
		]);
	}


	[Test]
	public void _1_ClickDoubleClick()
	{
		var sched = new TestScheduler();
		var actsRun = MkActsRun([actClick, actDoubleClick]);
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

		var obs = sched.CreateObserver<IActEvt>();
		using var d = new Disp();
		actsRun.ToActEvt(evt, sched, d).Subscribe(obs);

		sched.Start();
		obs.LogMessages("ActEvt");

		var delay = ActEvtGenerator.ClickDelay.TotalSeconds;

		obs.Messages.AssertEqual([
			OnNext(3.1 + delay, (IActEvt)new ConfirmActEvt(actClick, Pt.Zero)),
			OnNext(5.3, (IActEvt)new ConfirmActEvt(actDoubleClick, Pt.Zero)),
		]);
	}


	[Test]
	public void _2_RightClick()
	{
		var sched = new TestScheduler();
		var actsRun = MkActsRun([actRightClick]);
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

		var obs = sched.CreateObserver<IActEvt>();
		using var d = new Disp();
		actsRun.ToActEvt(evt, sched, d).Subscribe(obs);

		sched.Start();
		obs.LogMessages("ActEvt");

		obs.Messages.AssertEqual([
			OnNext(6.1, (IActEvt)new ConfirmActEvt(actRightClick, Pt.Zero)),
		]);
	}




	private static Recorded<Notification<IEvt>> Move(double t) => OnNext(t, (IEvt)new MouseMoveEvt(Pt.Zero));
	private static Recorded<Notification<IEvt>> LDown(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Down, MouseBtn.Left));
	private static Recorded<Notification<IEvt>> RDown(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Down, MouseBtn.Right));
	private static Recorded<Notification<IEvt>> LUp(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Up, MouseBtn.Left));
	private static Recorded<Notification<IEvt>> RUp(double t) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Up, MouseBtn.Right));
}