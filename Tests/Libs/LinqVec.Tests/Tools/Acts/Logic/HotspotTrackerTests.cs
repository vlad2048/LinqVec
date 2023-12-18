using LinqVec.Tools.Acts;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Events;
using Microsoft.Reactive.Testing;
using ReactiveVars;
using System.Reactive;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Acts.Logic;
using TestLib;
using H = System.Object;

namespace LinqVec.Tests.Tools.Acts.Logic;

class HotspotTrackerTests
{
	private static readonly IRoVar<ActSet> curActs = Var.MakeConst(new ActSet(
		"Main",
		CBase.Cursors.Pen,
		[
			new HotspotActs(
				new Hotspot("First", mouse => mouse.X switch {
					>= 0 and < 25 => Option<H>.Some("A"),
					>= 25 and < 50 => Option<H>.Some("B"),
					_ => None
				}, null),
				_ => [
					new HotspotAct("Drag", Gesture.Drag, HotspotActActions.Empty),
					new HotspotAct("Click", Gesture.Click, HotspotActActions.Empty),
				]
			),
			new HotspotActs(
				new Hotspot("Second", mouse => mouse.X switch {
					>= 50 and < 75 => Option<H>.Some("C"),
					>= 75 and < 100 => Option<H>.Some("D"),
					_ => None
				}, null),
				_ => [
					new HotspotAct("Drag", Gesture.Drag, HotspotActActions.Empty),
					new HotspotAct("Click", Gesture.Click, HotspotActActions.Empty),
				]
			),
		]
	));

	[Test]
	public void _0_Move()
	{
		var sched = new TestScheduler();
		var evt = sched.CreateHotObservable([
			Move(1, 10),
			Move(2, 20),
			Move(3, 30),
			Move(4, 60),
		]);
		using var d = new Disp();
		var curHotspot = curActs.TrackHotspot(evt, sched, d);

		var obs = sched.CreateObserver<(string, H)>();
		curHotspot
			.Select(e => (e.Hotspot.Name, e.HotspotValue))
			.Subscribe(obs);

		sched.Start();
		obs.LogMessages("curHotspot");

		obs.Messages.AssertEqual([
			OnNext(0, ("Empty", (H)null!)),
			OnNext(1, ("First", (H)"A")),
			OnNext(3, ("First", (H)"B")),
			OnNext(4, ("Second", (H)"C")),
		]);
	}

	[Test]
	public void _1_Drag()
	{
		var sched = new TestScheduler();
		var evt = sched.CreateHotObservable([
			Move(1, 10),
			LDown(2, 10),
			Move(3, 60),
			LUp(4, 60),
			Move(5, 80),
		]);
		using var d = new Disp();
		var curHotspot = curActs.TrackHotspot(evt, sched, d);

		var obs = sched.CreateObserver<(string, H)>();
		curHotspot
			.Select(e => (e.Hotspot.Name, e.HotspotValue))
			.Subscribe(obs);

		sched.Start();
		obs.LogMessages("curHotspot");

		obs.Messages.AssertEqual([
			OnNext(0, ("Empty", (H)null!)),
			OnNext(1, ("First", (H)"A")),
			OnNext(4, ("Second", (H)"C")),
			OnNext(5, ("Second", (H)"D")),
		]);
	}





	private static Recorded<Notification<IEvt>> Move(double t, int x) => OnNext(t, (IEvt)new MouseMoveEvt(new Pt(x, 0)));
	private static Recorded<Notification<IEvt>> LDown(double t, int x) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, 0), UpDown.Down, MouseBtn.Left));
	private static Recorded<Notification<IEvt>> RDown(double t, int x) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, 0), UpDown.Down, MouseBtn.Right));
	private static Recorded<Notification<IEvt>> LUp(double t, int x) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, 0), UpDown.Up, MouseBtn.Left));
	private static Recorded<Notification<IEvt>> RUp(double t, int x) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, 0), UpDown.Up, MouseBtn.Right));
}