using WinAPI.DwmApi;
using WinAPI.NetCoreEx.Geometry;
using WinAPI.User32;

// ReSharper disable once CheckNamespace
namespace SysWinLib;

public enum RType
{
	Win,
	WinWithGripAreas,
	Client,
}

public static class SysWinExt
{
	public static void Invalidate(this SysWin win) => User32Methods.InvalidateRect(win.Handle, nint.Zero, false);

	public static R GetR(this SysWin win, RType type)
	{
		Rectangle r;
		switch (type)
		{
			case RType.Win:
				DwmApiHelpers.DwmGetWindowAttribute(win.Handle, DwmWindowAttributeType.DWMWA_EXTENDED_FRAME_BOUNDS, out r);
				break;
			case RType.WinWithGripAreas:
				User32Methods.GetWindowRect(win.Handle, out r);
				break;
			case RType.Client:
				User32Methods.GetClientRect(win.Handle, out r);
				break;
			default:
				throw new ArgumentException($"Invalid RType: {type}");
		}
		return r.FromR();
	}

	public static void SetR(this SysWin win, R r, WindowPositionFlags flags) =>
		User32Methods.SetWindowPos(win.Handle, IntPtr.Zero, r.X, r.Y, r.Width, r.Height, flags);

	private static R FromR(this Rectangle r) => new(r.Left, r.Top, r.Width, r.Height);
}