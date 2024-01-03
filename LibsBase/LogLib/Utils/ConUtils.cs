using Geom;
using System.Drawing;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Local

namespace LogLib.Utils;

public static class ConUtils
{
	public static void Init()
	{
		AllocConsole();
		SetR(R.Make(-1400, 10, 800, 400));
		if (!EnableVirtualTerminalProcessing())
			Console.WriteLine("[Failed to initialize colors in the Console]");
	}

	public static R R
	{
		get => GetR();
		set => SetR(value);
	}

	public static void SetFore(Color c) => Console.Write($"{EscChar}[38;2;{c.R};{c.G};{c.B}m");
	public static void SetBack(Color c) => Console.Write($"{EscChar}[48;2;{c.R};{c.G};{c.B}m");


	private static R GetR()
	{
		var hWnd = GetConsoleWindow();
		GetWindowPlacement(hWnd, out var wp);
		var x1 = wp.NormalPosition.Left;
		var y1 = wp.NormalPosition.Top;
		var x2 = wp.NormalPosition.Right;
		var y2 = wp.NormalPosition.Bottom;
		return R.Make(x1, y1, x2 - x1, y2 - y1);
	}

	private static void SetR(R r) => SetWindowPos(GetConsoleWindow(), IntPtr.Zero, (int)r.Min.X, (int)r.Min.Y, (int)r.Width, (int)r.Height, SwpNozorder | SwpNoactivate);





	private const char EscChar = (char)0x1B;
	private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x04;

	private static bool EnableVirtualTerminalProcessing()
	{
		var hOut = GetStdHandle(unchecked((uint)(int)StdHandle.STD_OUTPUT_HANDLE));
		if (hOut == new IntPtr(-1))
			return false;
		var res = GetConsoleMode(hOut, out var mode);
		if (!res)
			return false;
		mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
		res = SetConsoleMode(hOut, mode);
		if (!res)
			return false;
		return true;
	}

	private enum StdHandle
	{
		/// <summary>
		///     The standard input device. Initially, this is the console input buffer, CONIN$.
		/// </summary>
		STD_INPUT_HANDLE = unchecked((int)(uint)-10),

		/// <summary>
		///     The standard output device. Initially, this is the active console screen buffer, CONOUT$.
		/// </summary>
		STD_OUTPUT_HANDLE = unchecked((int)(uint)-11),

		/// <summary>
		///     The standard error device. Initially, this is the active console screen buffer, CONOUT$.
		/// </summary>
		STD_ERROR_HANDLE = unchecked((int)(uint)-12)
	}

	[DllImport("kernel32", ExactSpelling = true)]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[DllImport("kernel32", ExactSpelling = true)]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	[DllImport("kernel32", ExactSpelling = true)]
	private static extern IntPtr GetStdHandle(uint nStdHandle);

	[DllImport("kernel32")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("user32")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

	private const int SwpNozorder = 0x4;
	private const int SwpNoactivate = 0x10;

	[DllImport("kernel32", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AllocConsole();

	[DllImport("user32", ExactSpelling = true)]
	private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);

	[DllImport("user32", ExactSpelling = true)]
	private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);

	[StructLayout(LayoutKind.Sequential)]
	private struct WindowPlacement
	{
		public uint Size;
		public WindowPlacementFlags Flags;
		public ShowWindowCommands ShowCmd;
		public Point MinPosition;
		public Point MaxPosition;
		public Rectangle NormalPosition;
	}

	[Flags]
	private enum WindowPlacementFlags
	{
		/// <summary>
		///     The coordinates of the minimized window may be specified.
		///     This flag must be specified if the coordinates are set in the ptMinPosition member.
		/// </summary>
		SETMINPOSITION = 0x0001,

		/// <summary>
		///     The restored window will be maximized, regardless of whether it was maximized before it was minimized. This setting
		///     is only valid the next time the window is restored. It does not change the default restoration behavior.
		///     This flag is only valid when the SW_SHOWMINIMIZED value is specified for the showCmd member.
		/// </summary>
		RESTORETOMAXIMIZED = 0x0002,

		/// <summary>
		///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts
		///     the request to the thread that owns the window. This prevents the calling thread from blocking its execution while
		///     other threads process the request.
		/// </summary>
		ASYNCWINDOWPLACEMENT = 0x0004
	}

	[Flags]
	private enum ShowWindowCommands
	{
		/// <summary>
		///     Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when
		///     minimizing windows from a different thread.
		/// </summary>
		SW_FORCEMINIMIZE = 11,

		/// <summary>
		///     Hides the window and activates another window.
		/// </summary>
		SW_HIDE = 0,

		/// <summary>
		///     Maximizes the specified window.
		/// </summary>
		SW_MAXIMIZE = 3,

		/// <summary>
		///     Minimizes the specified window and activates the next top-level window in the Z order.
		/// </summary>
		SW_MINIMIZE = 6,

		/// <summary>
		///     Activates and displays the window. If the window is minimized or maximized, the system restores it to its original
		///     size and position. An application should specify this flag when restoring a minimized window.
		/// </summary>
		SW_RESTORE = 9,

		/// <summary>
		///     Activates the window and displays it in its current size and position.
		/// </summary>
		SW_SHOW = 5,

		/// <summary>
		///     Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess
		///     function by the program that started the application.
		/// </summary>
		SW_SHOWDEFAULT = 10,

		/// <summary>
		///     Activates the window and displays it as a maximized window.
		/// </summary>
		SW_SHOWMAXIMIZED = 3,

		/// <summary>
		///     Activates the window and displays it as a minimized window.
		/// </summary>
		SW_SHOWMINIMIZED = 2,

		/// <summary>
		///     Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not
		///     activated.
		/// </summary>
		SW_SHOWMINNOACTIVE = 7,

		/// <summary>
		///     Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is
		///     not activated.
		/// </summary>
		SW_SHOWNA = 8,

		/// <summary>
		///     Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the
		///     window is not activated.
		/// </summary>
		SW_SHOWNOACTIVATE = 4,

		/// <summary>
		///     Activates and displays a window. If the window is minimized or maximized, the system restores it to its original
		///     size and position. An application should specify this flag when displaying the window for the first time.
		/// </summary>
		SW_SHOWNORMAL = 1
	}
}