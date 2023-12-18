/*
using System.Reactive;
using Geom;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Events;
using Microsoft.Reactive.Testing;
using TestLib;

namespace LinqVec.Tests.Tools.Acts.Events;

class ActEvtGeneratorTests
{
	[Test]
	public void _00_Drag()
	{
		var d = MkD();
		var sched = new TestScheduler();
		var src = sched.CreateColdObservable([
			Move(1, 10, 5),

			LDown(2, 10, 5),
			Move(3, 20, 5),
			Move(4, 30, 5),
			LUp(5, 30, 5),

			Move(6, 40, 5),

			LDown(7, 40, 5),
			Move(8, 50, 5),
			Move(9, 60, 5),
			LUp(10, 60, 5),

			Move(11, 70, 5),
		]);
		var actEvt = src.ToActEvt(Gesture.Drag, sched, d);
		var obs = sched.CreateObserver<IActEvt>();
		actEvt.Subscribe(obs);

		sched.Start();

		obs.LogMessages("ToActEvt");

		obs.Messages.AssertEqual([
			OnNext(3, (IActEvt)new DragStartActEvt(new Pt(10, 5))),
			OnNext(5, (IActEvt)new ConfirmActEvt(ConfirmType.DragEnd, new Pt(10, 5), new Pt(30, 5))),
			OnNext(8, (IActEvt)new DragStartActEvt(new Pt(40, 5))),
			OnNext(10, (IActEvt)new ConfirmActEvt(ConfirmType.DragEnd, new Pt(40, 5), new Pt(60, 5))),
		]);
	}

	[Test]
	public void _01_Click()
	{
		var d = MkD();
		var sched = new TestScheduler();
		var src = sched.CreateColdObservable([
			LDown(2, 10, 5),
			LUp(2.2, 10, 5),
		]);
		var actEvt = src.ToActEvt(Gesture.Click, sched, d);
		var obs = sched.CreateObserver<IActEvt>();
		actEvt.Subscribe(obs);

		sched.Start();

		obs.LogMessages("ToActEvt");

		obs.Messages.AssertEqual([
			OnNext(2.2, (IActEvt)new ConfirmActEvt(ConfirmType.Click, new Pt(10, 5), new Pt(10, 5))),
		]);
	}

	[Test]
	public void _02_ClickTooSlow()
	{
		var d = MkD();
		var sched = new TestScheduler();
		var src = sched.CreateColdObservable([
			LDown(2, 10, 5),
			LUp(3, 10, 5),
		]);
		var actEvt = src.ToActEvt(Gesture.Click, sched, d);
		var obs = sched.CreateObserver<IActEvt>();
		actEvt.Subscribe(obs);

		sched.Start();

		obs.LogMessages("ToActEvt");

		obs.Messages.AssertEqual([
		]);
	}


	[Test]
	public void _03_ClickAndDoubleClick_DoClick()
	{
		var d = MkD();
		var sched = new TestScheduler();
		var src = sched.CreateColdObservable([
			LDown(2, 10, 5),
			LUp(2.2, 10, 5),
		]);
		var actEvt = src.ToActEvt(Gesture.Click | Gesture.DoubleClick, sched, d);
		var obs = sched.CreateObserver<IActEvt>();
		actEvt.Subscribe(obs);

		sched.Start();

		obs.LogMessages("ToActEvt");

		obs.Messages.AssertEqual([
			OnNext(2.7, (IActEvt)new ConfirmActEvt(ConfirmType.Click, new Pt(10, 5), new Pt(10, 5))),
		]);
	}


	[Test]
	public void _04_ClickAndDoubleClick_DoDoubleClick()
	{
		var d = MkD();
		var sched = new TestScheduler();
		var src = sched.CreateColdObservable([
			LDown(2, 10, 5),
			LUp(2.2, 10, 5),
			LDown(2.6, 10, 5),
			LUp(3.0, 10, 5),
		]);
		var actEvt = src.ToActEvt(Gesture.Click | Gesture.DoubleClick, sched, d);
		var obs = sched.CreateObserver<IActEvt>();
		actEvt.Subscribe(obs);

		sched.Start();

		obs.LogMessages("ToActEvt");

		obs.Messages.AssertEqual([
			OnNext(3.0, (IActEvt)new ConfirmActEvt(ConfirmType.DoubleClick, new Pt(10, 5), new Pt(10, 5))),
		]);
	}


	[Test]
	public void _05_DragAndClickAndDoubleClick_DoDrag()
	{
		var d = MkD();
		var sched = new TestScheduler();
		var src = sched.CreateColdObservable([
			LDown(2, 10, 5),
			Move(2.1, 20, 5),
			LUp(2.2, 20, 5),
		]);
		var actEvt = src.ToActEvt(Gesture.Drag | Gesture.Click | Gesture.DoubleClick, sched, d);
		var obs = sched.CreateObserver<IActEvt>();
		actEvt.Subscribe(obs);

		sched.Start();

		obs.LogMessages("ToActEvt");

		obs.Messages.AssertEqual([
			OnNext(2.1, (IActEvt)new DragStartActEvt(new Pt(10, 5))),
			OnNext(2.2, (IActEvt)new ConfirmActEvt(ConfirmType.DragEnd, new Pt(10, 5), new Pt(20, 5))),
		]);
	}


	[Test]
	public void _06_DragAndClickAndDoubleClick_DoClick()
	{
		var d = MkD();
		var sched = new TestScheduler();
		var src = sched.CreateColdObservable([
			LDown(2, 10, 5),
			LUp(2.2, 10, 5),
		]);
		var actEvt = src.ToActEvt(Gesture.Drag | Gesture.Click | Gesture.DoubleClick, sched, d);
		var obs = sched.CreateObserver<IActEvt>();
		actEvt.Subscribe(obs);

		sched.Start();

		obs.LogMessages("ToActEvt");

		obs.Messages.AssertEqual([
			OnNext(2.7, (IActEvt)new ConfirmActEvt(ConfirmType.Click, new Pt(10, 5), new Pt(10, 5))),
		]);
	}

	private static Recorded<Notification<IEvt>> Move(double t, int x, int y) => OnNext(t, (IEvt)new MouseMoveEvt(new Pt(x, y)));
	private static Recorded<Notification<IEvt>> LDown(double t, int x, int y) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, y), UpDown.Down, MouseBtn.Left));
	private static Recorded<Notification<IEvt>> RDown(double t, int x, int y) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, y), UpDown.Down, MouseBtn.Right));
	private static Recorded<Notification<IEvt>> LUp(double t, int x, int y) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, y), UpDown.Up, MouseBtn.Left));
	private static Recorded<Notification<IEvt>> RUp(double t, int x, int y) => OnNext(t, (IEvt)new MouseBtnEvt(new Pt(x, y), UpDown.Up, MouseBtn.Right));
}
*/