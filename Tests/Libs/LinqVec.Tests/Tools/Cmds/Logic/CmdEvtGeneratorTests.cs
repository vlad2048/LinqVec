using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using Microsoft.Reactive.Testing;
using TestLib;

namespace LinqVec.Tests.Tools.Cmds.Logic;

class CmdEvtGeneratorTests
{
	private static readonly DragHotspotCmd cmdDrag = new("Drag", Gesture.Drag, DragHotspotCmd.EmptyAction);
	private static readonly ClickHotspotCmd cmdClick = new("Click", Gesture.Click, () => None);
	private static readonly ClickHotspotCmd cmdRightClick = new("RightClick", Gesture.RightClick, () => None);
	private static readonly ClickHotspotCmd cmdDoubleClick = new("DoubleClick", Gesture.DoubleClick, () => None);

	private static Hotspot MkHotspot(IHotspotCmd[] cmds) =>
		new(
			HotspotNfo.Empty,
			123,
			cmds,
			false
		);


	[Test]
	public void _0_SimpleDrag()
	{
		var sched = new TestScheduler();
		var hotspot = MkHotspot([cmdDrag]);

		var usrEvt = new[] {
			Move(false),
			Move(true),

			LDown(false),
			Move(false),
			Move(true),
			LUp(false),

			Move(true),
			Move(false),
		}.ToObservable();

		var obs = sched.CreateObserver<ICmdEvt>();
		using var d = new Disp();
		hotspot.ToCmdEvt(usrEvt, [], sched, d).Subscribe(obs);

		sched.Start();

		obs.AssertValues([
			new DragStartCmdEvt(cmdDrag, Pt.Zero),
			new DragFinishCmdEvt(cmdDrag, Pt.Zero, Pt.Zero),
		]);
	}



	[Test]
	public void _1_DragClick()
	{
		var sched = new TestScheduler();
		var hotspot = MkHotspot([cmdDrag, cmdClick]);

		var usrEvt = new[] {
			Move(false),

			LDown(false),
			Move(false),
			LUp(false),

			LDown(false),
			LUp(false),

			LDown(false),
			LUp(true),

			Move(false),
			LDown(false),
			Move(false),
			Move(false),
			LUp(false),
		}.ToObservable();

		var obs = sched.CreateObserver<ICmdEvt>();
		using var d = new Disp();
		hotspot.ToCmdEvt(usrEvt, [], sched, d).Subscribe(obs);

		sched.Start();

		obs.AssertValues([
			new DragStartCmdEvt(cmdDrag, Pt.Zero),
			new DragFinishCmdEvt(cmdDrag, Pt.Zero, Pt.Zero),
			new ConfirmCmdEvt(cmdClick, Pt.Zero),
			new DragStartCmdEvt(cmdDrag, Pt.Zero),
			new ConfirmCmdEvt(cmdDrag, Pt.Zero),
		]);
	}


	[Test]
	public void _2_ClickDoubleClick()
	{
		var sched = new TestScheduler();
		var hotspot = MkHotspot([cmdClick, cmdDoubleClick]);

		var usrEvt = new[] {
			Move(false),
			LDown(false),
			Move(true),
			LUp(true),

			LDown(false),
			LUp(true),

			LDown(false),
			LUp(true),
			LDown(true),
			LUp(true),
		}.ToObservable();


		var obs = sched.CreateObserver<ICmdEvt>();
		using var d = new Disp();
		hotspot.ToCmdEvt(usrEvt, [], sched, d).Subscribe(obs);

		sched.Start();

		obs.AssertValues([
			new ConfirmCmdEvt(cmdClick, Pt.Zero),
			new ConfirmCmdEvt(cmdDoubleClick, Pt.Zero),
		]);
	}


	[Test]
	public void _3_RightClick()
	{
		var sched = new TestScheduler();
		var actsRun = MkHotspot([cmdRightClick]);

		var usrEvt = new[] {
			Move(false),
			LDown(false),
			LUp(true),

			RDown(false),
			RUp(false),

			RDown(false),
			RUp(true),

			RDown(false),
			Move(true),
			RUp(true),
		}.ToObservable();

		var obs = sched.CreateObserver<ICmdEvt>();
		using var d = new Disp();
		actsRun.ToCmdEvt(usrEvt, [], sched, d).Subscribe(obs);

		sched.Start();

		obs.AssertValues([
			new ConfirmCmdEvt(cmdRightClick, Pt.Zero)
		]);
	}


	private static IUsr Move(bool quick) => new MoveUsr(quick, Pt.Zero);
	private static IUsr LDown(bool quick) => new LDownUsr(quick, Pt.Zero, ModKeyState.Empty);
	private static IUsr RDown(bool quick) => new RDownUsr(quick, Pt.Zero);
	private static IUsr LUp(bool quick) => new LUpUsr(quick, Pt.Zero, ModKeyState.Empty);
	private static IUsr RUp(bool quick) => new RUpUsr(quick, Pt.Zero);


	/*private static Recorded<Notification<IEvt>> Move(bool quick) => OnNext(t, (IEvt)new MouseMoveEvt(Pt.Zero));
	private static Recorded<Notification<IEvt>> LDown(bool quick) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Down, MouseBtn.Left, ModKeyState.Empty));
	private static Recorded<Notification<IEvt>> RDown(bool quick) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Down, MouseBtn.Right, ModKeyState.Empty));
	private static Recorded<Notification<IEvt>> LUp(bool quick) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Up, MouseBtn.Left, ModKeyState.Empty));
	private static Recorded<Notification<IEvt>> RUp(bool quick) => OnNext(t, (IEvt)new MouseBtnEvt(Pt.Zero, UpDown.Up, MouseBtn.Right, ModKeyState.Empty));*/
}
