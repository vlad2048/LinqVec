using System.Diagnostics;
using System.Runtime.InteropServices;
using SysWinLib.Structs;
using WinAPI.User32;

namespace SysWinLib.Utils;

public static class RegisterClassUtils
{
	internal static readonly IntPtr InstanceHandle = Process.GetCurrentProcess().Handle;
	private static readonly IntPtr Icon = User32Helpers.LoadIcon(IntPtr.Zero, SystemIcon.IDI_APPLICATION);
	private static readonly IntPtr Cursor = User32Helpers.LoadCursor(IntPtr.Zero, SystemCursor.IDC_ARROW);
	private static readonly Dictionary<string, WindowProc> registered = new();

	public static string Register(string name, RegisterClassParams ps)
	{
		if (registered.ContainsKey(name)) throw new ArgumentException($"Window class '{name}' already registered");
		var winProc = ps.WinProc ?? User32Methods.DefWindowProc;
		registered[name] = winProc;

		var cls = new WindowClassEx
		{
			Size = (uint)Marshal.SizeOf(typeof(WindowClassEx)),

			ClassName = name,
			InstanceHandle = InstanceHandle,
			WindowProc = winProc,

			Styles = ps.Styles,
			IconHandle = ps.Icon ?? Icon,
			SmallIconHandle = ps.SmallIcon ?? IntPtr.Zero,
			CursorHandle = ps.Cursor ?? Cursor,
			MenuName = ps.Menu ?? null,
			BackgroundBrushHandle = ps.BackgroundBrush ?? IntPtr.Zero,
		};

		var res = User32Methods.RegisterClassEx(ref cls);
		ErrorUtils.Check(res, "RegisterClassEx");

		return name;
	}
}