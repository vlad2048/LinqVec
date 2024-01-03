using WinAPI.User32;

namespace SysWinLib;

public static class App
{
	internal static bool IsWinDXAppRunning { get; private set; }

	public static void Run()
	{
		IsWinDXAppRunning = true;
		while (User32Methods.GetMessage(out var msg, IntPtr.Zero, 0, 0) != 0)
		{
			User32Methods.TranslateMessage(ref msg);
			User32Methods.DispatchMessage(ref msg);
		}
	}
}