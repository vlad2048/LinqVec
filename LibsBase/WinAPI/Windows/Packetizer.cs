using System.Reactive.Linq;
using WinAPI.User32;

namespace WinAPI.Windows;

public static class Packetizer
{
	public static unsafe IPacket MakePacket(ref WindowMessage msg)
	{
		fixed (WindowMessage* ptr = &msg)
		{
			return msg.Id switch
			{
				WM.PAINT => new PaintPacket(ptr),
				WM.NCDESTROY => new Packet(ptr),
				WM.CLOSE => new Packet(ptr),
				WM.TIMECHANGE => new Packet(ptr),
				WM.DESTROY => new Packet(ptr),
				WM.MOUSELEAVE => new Packet(ptr),
				WM.NCACTIVATE => new NcActivatePacket(ptr),
				WM.NCCALCSIZE => new NcCalcSizePacket(ptr),
				WM.SHOWWINDOW => new ShowWindowPacket(ptr),
				WM.QUIT => new QuitPacket(ptr),
				WM.CREATE => new CreateWindowPacket(ptr),
				WM.SIZE => new SizePacket(ptr),
				WM.MOVE => new MovePacket(ptr),
				WM.WINDOWPOSCHANGED => new WindowPositionPacket(ptr),
				WM.WINDOWPOSCHANGING => new WindowPositionPacket(ptr),
				WM.ACTIVATE => new ActivatePacket(ptr),
				WM.ERASEBKGND => new EraseBkgndPacket(ptr),
				WM.ACTIVATEAPP => new ActivateAppPacket(ptr),
				WM.DISPLAYCHANGE => new DisplayChangePacket(ptr),
				WM.MOUSEMOVE => new MousePacket(ptr),
				WM.LBUTTONUP => new MouseButtonPacket(ptr),
				WM.LBUTTONDOWN => new MouseButtonPacket(ptr),
				WM.LBUTTONDBLCLK => new MouseDoubleClickPacket(ptr),
				WM.RBUTTONUP => new MouseButtonPacket(ptr),
				WM.RBUTTONDOWN => new MouseButtonPacket(ptr),
				WM.RBUTTONDBLCLK => new MouseDoubleClickPacket(ptr),
				WM.MBUTTONUP => new MouseButtonPacket(ptr),
				WM.MBUTTONDOWN => new MouseButtonPacket(ptr),
				WM.MBUTTONDBLCLK => new MouseDoubleClickPacket(ptr),
				WM.XBUTTONUP => new MouseButtonPacket(ptr),
				WM.XBUTTONDOWN => new MouseButtonPacket(ptr),
				WM.XBUTTONDBLCLK => new MouseDoubleClickPacket(ptr),
				WM.MOUSEACTIVATE => new MouseActivatePacket(ptr),
				WM.MOUSEHOVER => new MousePacket(ptr),
				WM.MOUSEWHEEL => new MouseWheelPacket(ptr),
				WM.MOUSEHWHEEL => new MouseWheelPacket(ptr),
				WM.CHAR => new KeyCharPacket(ptr),
				WM.SYSCHAR => new KeyCharPacket(ptr),
				WM.DEADCHAR => new KeyCharPacket(ptr),
				WM.SYSDEADCHAR => new KeyCharPacket(ptr),
				WM.KEYUP => new KeyPacket(ptr),
				WM.KEYDOWN => new KeyPacket(ptr),
				WM.SYSKEYUP => new KeyPacket(ptr),
				WM.SYSKEYDOWN => new KeyPacket(ptr),
				WM.SYSCOMMAND => new SysCommandPacket(ptr),
				WM.MENUCOMMAND => new MenuCommandPacket(ptr),
				WM.APPCOMMAND => new AppCommandPacket(ptr),
				WM.KILLFOCUS => new FocusPacket(ptr),
				WM.SETFOCUS => new FocusPacket(ptr),
				WM.CAPTURECHANGED => new CaptureChangedPacket(ptr),
				WM.NCHITTEST => new NcHitTestPacket(ptr),
				WM.HOTKEY => new HotKeyPacket(ptr),
				WM.GETMINMAXINFO => new MinMaxInfoPacket(ptr),
				WM.NCPAINT => new NcPaintPacket(ptr),
				WM.NCMOUSEMOVE => new NcMouseMovePacket(ptr),

				_ => new Packet(ptr),
			};
		}
	}


	public static IObservable<PaintPacket> WhenPAINT(this IObservable<IPacket> obs) => obs.WhenId<PaintPacket>(WM.PAINT);
	public static IObservable<Packet> WhenNCDESTROY(this IObservable<IPacket> obs) => obs.WhenId<Packet>(WM.NCDESTROY);
	public static IObservable<Packet> WhenCLOSE(this IObservable<IPacket> obs) => obs.WhenId<Packet>(WM.CLOSE);
	public static IObservable<Packet> WhenTIMECHANGE(this IObservable<IPacket> obs) => obs.WhenId<Packet>(WM.TIMECHANGE);
	public static IObservable<Packet> WhenDESTROY(this IObservable<IPacket> obs) => obs.WhenId<Packet>(WM.DESTROY);
	public static IObservable<Packet> WhenMOUSELEAVE(this IObservable<IPacket> obs) => obs.WhenId<Packet>(WM.MOUSELEAVE);
	public static IObservable<NcActivatePacket> WhenNCACTIVATE(this IObservable<IPacket> obs) => obs.WhenId<NcActivatePacket>(WM.NCACTIVATE);
	public static IObservable<NcCalcSizePacket> WhenNCCALCSIZE(this IObservable<IPacket> obs) => obs.WhenId<NcCalcSizePacket>(WM.NCCALCSIZE);
	public static IObservable<ShowWindowPacket> WhenSHOWWINDOW(this IObservable<IPacket> obs) => obs.WhenId<ShowWindowPacket>(WM.SHOWWINDOW);
	public static IObservable<QuitPacket> WhenQUIT(this IObservable<IPacket> obs) => obs.WhenId<QuitPacket>(WM.QUIT);
	public static IObservable<CreateWindowPacket> WhenCREATE(this IObservable<IPacket> obs) => obs.WhenId<CreateWindowPacket>(WM.CREATE);
	public static IObservable<SizePacket> WhenSIZE(this IObservable<IPacket> obs) => obs.WhenId<SizePacket>(WM.SIZE);
	public static IObservable<MovePacket> WhenMOVE(this IObservable<IPacket> obs) => obs.WhenId<MovePacket>(WM.MOVE);
	public static IObservable<WindowPositionPacket> WhenWINDOWPOSCHANGED(this IObservable<IPacket> obs) => obs.WhenId<WindowPositionPacket>(WM.WINDOWPOSCHANGED);
	public static IObservable<WindowPositionPacket> WhenWINDOWPOSCHANGING(this IObservable<IPacket> obs) => obs.WhenId<WindowPositionPacket>(WM.WINDOWPOSCHANGING);
	public static IObservable<ActivatePacket> WhenACTIVATE(this IObservable<IPacket> obs) => obs.WhenId<ActivatePacket>(WM.ACTIVATE);
	public static IObservable<EraseBkgndPacket> WhenERASEBKGND(this IObservable<IPacket> obs) => obs.WhenId<EraseBkgndPacket>(WM.ERASEBKGND);
	public static IObservable<ActivateAppPacket> WhenACTIVATEAPP(this IObservable<IPacket> obs) => obs.WhenId<ActivateAppPacket>(WM.ACTIVATEAPP);
	public static IObservable<DisplayChangePacket> WhenDISPLAYCHANGE(this IObservable<IPacket> obs) => obs.WhenId<DisplayChangePacket>(WM.DISPLAYCHANGE);
	public static IObservable<MousePacket> WhenMOUSEMOVE(this IObservable<IPacket> obs) => obs.WhenId<MousePacket>(WM.MOUSEMOVE);
	public static IObservable<MouseButtonPacket> WhenLBUTTONUP(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.LBUTTONUP);
	public static IObservable<MouseButtonPacket> WhenLBUTTONDOWN(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.LBUTTONDOWN);
	public static IObservable<MouseDoubleClickPacket> WhenLBUTTONDBLCLK(this IObservable<IPacket> obs) => obs.WhenId<MouseDoubleClickPacket>(WM.LBUTTONDBLCLK);
	public static IObservable<MouseButtonPacket> WhenRBUTTONUP(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.RBUTTONUP);
	public static IObservable<MouseButtonPacket> WhenRBUTTONDOWN(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.RBUTTONDOWN);
	public static IObservable<MouseDoubleClickPacket> WhenRBUTTONDBLCLK(this IObservable<IPacket> obs) => obs.WhenId<MouseDoubleClickPacket>(WM.RBUTTONDBLCLK);
	public static IObservable<MouseButtonPacket> WhenMBUTTONUP(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.MBUTTONUP);
	public static IObservable<MouseButtonPacket> WhenMBUTTONDOWN(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.MBUTTONDOWN);
	public static IObservable<MouseDoubleClickPacket> WhenMBUTTONDBLCLK(this IObservable<IPacket> obs) => obs.WhenId<MouseDoubleClickPacket>(WM.MBUTTONDBLCLK);
	public static IObservable<MouseButtonPacket> WhenXBUTTONUP(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.XBUTTONUP);
	public static IObservable<MouseButtonPacket> WhenXBUTTONDOWN(this IObservable<IPacket> obs) => obs.WhenId<MouseButtonPacket>(WM.XBUTTONDOWN);
	public static IObservable<MouseDoubleClickPacket> WhenXBUTTONDBLCLK(this IObservable<IPacket> obs) => obs.WhenId<MouseDoubleClickPacket>(WM.XBUTTONDBLCLK);
	public static IObservable<MouseActivatePacket> WhenMOUSEACTIVATE(this IObservable<IPacket> obs) => obs.WhenId<MouseActivatePacket>(WM.MOUSEACTIVATE);
	public static IObservable<MousePacket> WhenMOUSEHOVER(this IObservable<IPacket> obs) => obs.WhenId<MousePacket>(WM.MOUSEHOVER);
	public static IObservable<MouseWheelPacket> WhenMOUSEWHEEL(this IObservable<IPacket> obs) => obs.WhenId<MouseWheelPacket>(WM.MOUSEWHEEL);
	public static IObservable<MouseWheelPacket> WhenMOUSEHWHEEL(this IObservable<IPacket> obs) => obs.WhenId<MouseWheelPacket>(WM.MOUSEHWHEEL);
	public static IObservable<KeyCharPacket> WhenCHAR(this IObservable<IPacket> obs) => obs.WhenId<KeyCharPacket>(WM.CHAR);
	public static IObservable<KeyCharPacket> WhenSYSCHAR(this IObservable<IPacket> obs) => obs.WhenId<KeyCharPacket>(WM.SYSCHAR);
	public static IObservable<KeyCharPacket> WhenDEADCHAR(this IObservable<IPacket> obs) => obs.WhenId<KeyCharPacket>(WM.DEADCHAR);
	public static IObservable<KeyCharPacket> WhenSYSDEADCHAR(this IObservable<IPacket> obs) => obs.WhenId<KeyCharPacket>(WM.SYSDEADCHAR);
	public static IObservable<KeyPacket> WhenKEYUP(this IObservable<IPacket> obs) => obs.WhenId<KeyPacket>(WM.KEYUP);
	public static IObservable<KeyPacket> WhenKEYDOWN(this IObservable<IPacket> obs) => obs.WhenId<KeyPacket>(WM.KEYDOWN);
	public static IObservable<KeyPacket> WhenSYSKEYUP(this IObservable<IPacket> obs) => obs.WhenId<KeyPacket>(WM.SYSKEYUP);
	public static IObservable<KeyPacket> WhenSYSKEYDOWN(this IObservable<IPacket> obs) => obs.WhenId<KeyPacket>(WM.SYSKEYDOWN);
	public static IObservable<SysCommandPacket> WhenSYSCOMMAND(this IObservable<IPacket> obs) => obs.WhenId<SysCommandPacket>(WM.SYSCOMMAND);
	public static IObservable<MenuCommandPacket> WhenMENUCOMMAND(this IObservable<IPacket> obs) => obs.WhenId<MenuCommandPacket>(WM.MENUCOMMAND);
	public static IObservable<AppCommandPacket> WhenAPPCOMMAND(this IObservable<IPacket> obs) => obs.WhenId<AppCommandPacket>(WM.APPCOMMAND);
	public static IObservable<FocusPacket> WhenKILLFOCUS(this IObservable<IPacket> obs) => obs.WhenId<FocusPacket>(WM.KILLFOCUS);
	public static IObservable<FocusPacket> WhenSETFOCUS(this IObservable<IPacket> obs) => obs.WhenId<FocusPacket>(WM.SETFOCUS);
	public static IObservable<CaptureChangedPacket> WhenCAPTURECHANGED(this IObservable<IPacket> obs) => obs.WhenId<CaptureChangedPacket>(WM.CAPTURECHANGED);
	public static IObservable<NcHitTestPacket> WhenNCHITTEST(this IObservable<IPacket> obs) => obs.WhenId<NcHitTestPacket>(WM.NCHITTEST);
	public static IObservable<HotKeyPacket> WhenHOTKEY(this IObservable<IPacket> obs) => obs.WhenId<HotKeyPacket>(WM.HOTKEY);
	public static IObservable<MinMaxInfoPacket> WhenGETMINMAXINFO(this IObservable<IPacket> obs) => obs.WhenId<MinMaxInfoPacket>(WM.GETMINMAXINFO);
	public static IObservable<NcPaintPacket> WhenNCPAINT(this IObservable<IPacket> obs) => obs.WhenId<NcPaintPacket>(WM.NCPAINT);
	public static IObservable<NcMouseMovePacket> WhenNCMOUSEMOVE(this IObservable<IPacket> obs) => obs.WhenId<NcMouseMovePacket>(WM.NCMOUSEMOVE);


	private static IObservable<T> WhenId<T>(this IObservable<IPacket> obs, WM id) where T : IPacket =>
		obs
			.OfType<T>()
			.Where(e => e.MsgId == id);
}