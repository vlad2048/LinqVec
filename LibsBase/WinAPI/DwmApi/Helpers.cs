using System.Runtime.InteropServices;
using WinAPI.Core;
using WinAPI.NetCoreEx.Geometry;

namespace WinAPI.DwmApi;

public static class DwmApiHelpers
{
	public static unsafe HResult DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttributeType dwAttribute, [In] ref int attrValue)
	{
		fixed (int* ptr = &attrValue) return DwmApiMethods.DwmSetWindowAttribute(hwnd, dwAttribute, new IntPtr(ptr), sizeof(int));
	}

	public static unsafe HResult DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttributeType dwAttribute, out Rectangle rect)
	{
		fixed (Rectangle* ptr = &rect) return DwmApiMethods.DwmGetWindowAttribute(hwnd, dwAttribute, new IntPtr(ptr), (uint) sizeof(Rectangle));
	}
}