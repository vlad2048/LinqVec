using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using ReactiveVars;
using SysWinInterfaces;
using SysWinLib.Utils;
using WinAPI.User32;
using WinAPI.Windows;

namespace SysWinLib;


/// <summary>
/// Low level Win32 window wrapper
/// </summary>
public sealed class SysWin : ISysWin
{
	// static
	private static bool isFirst = true;

	// IDisposable
	public Disp D { get; } = new();
	public void Dispose()
	{
		if (!hasDestroyBeenSent)
			Destroy();
		else
			D.Dispose();
	}
	private void Destroy()
	{
		if (hasDestroyBeenSent) return;
		User32Methods.DestroyWindow(Handle);
		hasDestroyBeenSent = true;		// hasDestroyBeenSent should already be true when DestroyWindow returns, but just in case we set it here
	}

	// private
	private readonly SysWinOpt opt;
	private readonly bool isMainWindow;
	private readonly ISubject<IPacket> whenMsg;
	private GCHandle gcHandle;
	private bool hasDestroyBeenSent;

	// public
	public IntPtr Handle { get; private set; }
	public IObservable<IPacket> WhenMsg => whenMsg.AsObservable();
	public IRoVar<bool> IsInit { get; }
	public IRoVar<Pt> ScreenPt { get; }
	public IRoVar<R> ScreenR { get; }
	public IRoVar<Sz> ClientSz { get; }

	public SysWin(
		Action<SysWinOpt>? optFun = null
	)
	{
		opt = SysWinOpt.Make(optFun);
		isMainWindow = isFirst;
		isFirst = false;
		whenMsg = new Subject<IPacket>().D(D);

		// flagging WM_WINDOWPOSCHANGED as handled (not calling the default windowproc) means we won't receive WM_SIZE & WM_MOVE messages
		// (but we're not doing that here)

		// This observation was made with our custom NonClientArea
		// =======================================================
		// RType.Client				: not quite right 240x217 (instead of 256x256)
		// RType.Win				: correct 256x256
		// RType.WinWithGripAreas	: correct 256x256

		ClientSz =
			Obs.Merge(
					// hooking to WM_CREATE is just so other code doesn't get ClientR=Empty in WM_CREATE.
					WhenMsg.WhenCREATE().Select(_ => this.GetR(RType.WinWithGripAreas).Size),
					WhenMsg.WhenWINDOWPOSCHANGED().Select(e => new Sz(e.Position.Width, e.Position.Height))
				)
				.Prepend(Sz.Empty)
				.ToVar(D);

		ScreenPt =
			WhenMsg.WhenWINDOWPOSCHANGED().Select(e => new Pt(e.Position.X, e.Position.Y))
				.Prepend(Pt.Empty)
				.ToVar(D);

		ScreenR = Obs.CombineLatest(ScreenPt, ClientSz, (screenPt, clientSz) => (screenPt, clientSz))
			.Select(t => new R(t.screenPt, t.clientSz)).ToVar();

		IsInit = WhenMsg.WhenCREATE().Select(_ => true).Prepend(false).ToVar(D);

		gcHandle = GCHandle.Alloc(this);
		Disposable.Create(gcHandle.Free).D(D);

		this.ApplyNCStrat(opt.NCStrat).D(D);
		this.GenerateMouseLeaveMessagesIFN(opt);
		WhenMsg.WhenERASEBKGND().Subscribe(e => e.MarkAsHandled()).D(D);

		//WhenMsg
		//	.Where(e => e.MsgId == WM.MOUSEMOVE)
		//	.Subscribe(e => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] sys: {e.MsgId}")).D(D);
	}

	public void Init()
	{
		try
		{
			User32Methods.CreateWindowEx(
				opt.CreateWindowParams.ExStyles,
				opt.WinClass,
				opt.CreateWindowParams.Name,
				opt.CreateWindowParams.Styles,
				opt.CreateWindowParams.X,
				opt.CreateWindowParams.Y,
				opt.CreateWindowParams.Width,
				opt.CreateWindowParams.Height,
				opt.CreateWindowParams.Parent,
				opt.CreateWindowParams.Menu,
				RegisterClassUtils.InstanceHandle,
				GCHandle.ToIntPtr(gcHandle)
			);
		}
		finally
		{
			//Check(Handle, "CreateWindowEx");
		}
	}


	public static IntPtr WndProc(IntPtr hWnd, WM id, IntPtr wParam, IntPtr lParam)
	{
	    var msg = new WindowMessage
	    {
		    Id = id,
		    WParam = wParam,
		    LParam = lParam,
		    Result = IntPtr.Zero,
		    Hwnd = hWnd
	    };
	    IntPtr DefProc() => User32Methods.DefWindowProc(msg.Hwnd, msg.Id, msg.WParam, msg.LParam);

		// 1. Extract the SysWin instance from WM_NCCREATE message and write it in the window extra params for subsequent messages
	    var win = ExtractWinFromMsg(msg);
	    if (win == null) return DefProc();

		// 2. Watch for WM_NCDESTROY. When it arrives, destroy the window and exit the message loop if this is the main window
		if (win.HandleDestroy(msg)) return IntPtr.Zero;

		// 3. Broadcast the message to WhenMsg
		win.whenMsg.OnNext(Packetizer.MakePacket(ref msg));
		// 4. If no receiver to the broadcast flagged the message as handled, send it to the default window procedure
		var finalRes = msg.Handled switch
		{
			true => msg.Result,
			false => DefProc()
		};
		return finalRes;
	}


	private bool HandleDestroy(WindowMessage msg)
	{
		if (msg.Id != WM.NCDESTROY) return false;
		hasDestroyBeenSent = true;
		if (isMainWindow && App.IsWinDXAppRunning)
		{
			User32Methods.PostQuitMessage(0);
		}

		Dispose();

		return true;
	}


	private static unsafe SysWin? ExtractWinFromMsg(WindowMessage msg)
	{
		SysWin win;
		if (msg.Id == WM.NCCREATE)
		{
			var createStruct = *(CreateStruct*) msg.LParam.ToPointer();
			var winPtr = createStruct.CreateParams;
			User32Helpers.SetWindowLongPtr(msg.Hwnd, WindowLongFlags.GWLP_USERDATA, winPtr);
			win = winPtr.GetWinFromPtr<SysWin>();
			win.Handle = msg.Hwnd;
		}
		else
		{
			var winPtr = User32Helpers.GetWindowLongPtr(msg.Hwnd, WindowLongFlags.GWLP_USERDATA);
			if (winPtr == IntPtr.Zero)
				return null;
			win = winPtr.GetWinFromPtr<SysWin>();
		}
		return win;
	}
}


file static class SysWinPrivateExt
{
	public static T GetWinFromPtr<T>(this IntPtr ptr) where T : class
	{
		var handle = GCHandle.FromIntPtr(ptr);
		var resObj = handle.Target ?? throw new NullReferenceException("handle shouldn't be null");
		var res = resObj as T ?? throw new NullReferenceException("handle is of the wrong type");
		return res;
	}
}