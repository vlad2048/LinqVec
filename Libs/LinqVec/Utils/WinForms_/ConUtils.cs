using System.Runtime.InteropServices;

namespace LinqVec.Utils.WinForms_;

public static class ConUtils
{
	public static void Init()
	{
		AllocConsole();
		SetRect(MkR(-1400, 10, 800, 400));
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AllocConsole();

	private static void SetRect(RInt r)
	{
		SetWindowPos(GetConsoleWindow(), IntPtr.Zero, r.Min.X, r.Min.Y, r.Width, r.Height, SwpNozorder | SwpNoactivate);
	}


	[DllImport("kernel32")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("user32")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

	private const int SwpNozorder = 0x4;
	private const int SwpNoactivate = 0x10;
}