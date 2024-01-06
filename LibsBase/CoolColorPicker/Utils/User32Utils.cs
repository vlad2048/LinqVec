using System.Runtime.InteropServices;

namespace CoolColorPicker.Utils;

static class User32Utils
{
	[DllImport("user32", ExactSpelling = true)]
	public static extern bool ReleaseCapture();

	[DllImport("user32", ExactSpelling = true)]
	public static extern IntPtr SetCapture(IntPtr hWnd);
}