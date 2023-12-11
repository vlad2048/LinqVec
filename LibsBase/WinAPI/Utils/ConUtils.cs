using System.Drawing;
using Geom;
using WinAPI.Kernel32;
using WinAPI.User32;

namespace WinAPI.Utils;

public static class ConUtils
{
	private const char EscChar = (char)0x1B;
	private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x04;

	
	public static void Init(R r)
	{
		Kernel32Methods.AllocConsole();
		if (!EnableVirtualTerminalProcessing()) throw new ArgumentException("Console EnableVirtualTerminalProcessing() failed");
		SetR(r);
	}

	public static R GetR()
	{
		var hWnd = Kernel32Methods.GetConsoleWindow();
		//User32Helpers.GetMonitorInfo(hWnd, out var mi);
		User32Methods.GetWindowPlacement(hWnd, out var wp);
		var x1 = wp.NormalPosition.Left;
		var y1 = wp.NormalPosition.Top;
		var x2 = wp.NormalPosition.Right;
		var y2 = wp.NormalPosition.Bottom;
		return R.Make(x1, y1, x2 - x1, y2 - y1);
	}

	public static void SetR(R r)
	{
		if (r == R.Empty) return;
		var hWnd = Kernel32Methods.GetConsoleWindow();
		//User32Helpers.GetMonitorInfo(hWnd, out var mi);
		User32Methods.GetWindowPlacement(hWnd, out var wp);
		wp.NormalPosition = r.ToWinR();
		User32Methods.SetWindowPlacement(hWnd, ref wp);
	}
	private static NetCoreEx.Geometry.Rectangle ToWinR(this R r) => new((int)r.Min.X, (int)r.Min.Y, (int)r.Width, (int)r.Height);

	public static void SetColor(Color c) => Console.Write($"{EscChar}[38;2;{c.R};{c.G};{c.B}m");


	private static bool EnableVirtualTerminalProcessing()
	{
		var hOut = Kernel32Methods.GetStdHandle(unchecked((uint) (int) StdHandle.STD_OUTPUT_HANDLE));
		if (hOut == new IntPtr(-1))
			return false;
		var res = Kernel32Methods.GetConsoleMode(hOut, out var mode);
		if (!res)
			return false;
		mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
		res = Kernel32Methods.SetConsoleMode(hOut, mode);
		if (!res)
			return false;
		return true;
	}
}