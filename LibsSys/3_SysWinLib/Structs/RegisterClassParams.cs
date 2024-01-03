using WinAPI.User32;

namespace SysWinLib.Structs;

public sealed class RegisterClassParams
{
	public WindowClassStyles Styles { get; init; }
	public IntPtr? Icon { get; init; }
	public IntPtr? SmallIcon { get; init; }
	public IntPtr? Cursor { get; init; }
	public string? Menu { get; init; }
	public IntPtr? BackgroundBrush { get; init; }
	public WindowProc? WinProc { get; init; }
}