using System.Runtime.InteropServices;
using Geom;

namespace LinqVec.Utils.WinForms_;

public static class ConUtils
{
	public static void Init()
	{
		AllocConsole();
		SetRect(R.Make(-1400, 10, 800, 400));
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AllocConsole();

	private static void SetRect(R r)
	{
		SetWindowPos(GetConsoleWindow(), IntPtr.Zero, (int)r.Min.X, (int)r.Min.Y, (int)r.Width, (int)r.Height, SwpNozorder | SwpNoactivate);
	}


	[DllImport("kernel32")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("user32")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

	private const int SwpNozorder = 0x4;
	private const int SwpNoactivate = 0x10;
}