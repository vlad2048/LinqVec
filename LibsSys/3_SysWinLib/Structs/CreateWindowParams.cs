using WinAPI.User32;

namespace SysWinLib.Structs;

public sealed class CreateWindowParams
{
	public int X { get; init; } = (int)CreateWindowFlags.CW_USEDEFAULT;
	public int Y { get; init; } = (int)CreateWindowFlags.CW_USEDEFAULT;
	public int Width { get; init; } = (int)CreateWindowFlags.CW_USEDEFAULT;
	public int Height { get; init; } = (int)CreateWindowFlags.CW_USEDEFAULT;
	public string Name { get; init; } = string.Empty;
	public IntPtr Parent { get; init; }
	public IntPtr Menu { get; init; }
	public WindowStyles Styles { get; init; } = WindowStyles.WS_VISIBLE;
	public WindowExStyles ExStyles { get; init; }
	public uint ControlStyles { get; init; }
}