using System.Reactive;
using Geom;
using LinqVec.Tools.Events;
using Microsoft.Reactive.Testing;

namespace LinqVec.Tests.TestSupport;

static class EvtGen
{
	private static Pt lastPt = Pt.Zero;

	public static void ResetEvtGen() => lastPt = Pt.Zero;

	public static Recorded<Notification<IEvt>> Move(double t, int pt) => OnNext(t, (IEvt)new MouseMoveEvt(lastPt = new Pt(pt, 0)));
	public static Recorded<Notification<IEvt>> LDown(double t) => OnNext(t, (IEvt)new MouseBtnEvt(lastPt, UpDown.Down, MouseBtn.Left, ModKeyState.Empty));
	public static Recorded<Notification<IEvt>> RDown(double t) => OnNext(t, (IEvt)new MouseBtnEvt(lastPt, UpDown.Down, MouseBtn.Right, ModKeyState.Empty));
	public static Recorded<Notification<IEvt>> LUp(double t) => OnNext(t, (IEvt)new MouseBtnEvt(lastPt, UpDown.Up, MouseBtn.Left, ModKeyState.Empty));
	public static Recorded<Notification<IEvt>> RUp(double t) => OnNext(t, (IEvt)new MouseBtnEvt(lastPt, UpDown.Up, MouseBtn.Right, ModKeyState.Empty));
}
