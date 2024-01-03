using WinAPI.Core;
using WinAPI.Kernel32;

namespace SysWinLib.Utils;

static class ErrorUtils
{
	public static void Check(ushort res, string name)
	{
		if (res != 0) return;
		Check(name);
	}

	public static void Check(IntPtr res, string name)
	{
		if (res != IntPtr.Zero) return;
		Check(name);
	}

	public static void Check(HResult res, string name)
	{
		if (res == HResult.S_OK) return;
		Check($"{name} (hres={res})");
	}

	private static void Check(string name)
	{
		var err = Kernel32Methods.GetLastError();
		throw new ApplicationException($"Windows error in {name}: {err} (0x{err:x})");
	}
}