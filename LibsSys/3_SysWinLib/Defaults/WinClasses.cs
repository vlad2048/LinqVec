using SysWinLib.Structs;
using SysWinLib.Utils;
using WinAPI.Gdi32;
using WinAPI.User32;

namespace SysWinLib.Defaults;

public static class WinClasses
{
	public static string MainWindow => mainWindow.Value;
	public static string PopupWindow => popupWindow.Value;

	private static readonly Lazy<string> mainWindow = new(() => RegisterClassUtils.Register(
		"MainWindow",
		new RegisterClassParams
		{
			Styles =
				WindowClassStyles.CS_HREDRAW |
				WindowClassStyles.CS_VREDRAW |
				WindowClassStyles.CS_OWNDC |
				0
			,
			BackgroundBrush = Gdi32Helpers.GetStockObject(StockObject.BLACK_BRUSH),
			WinProc = SysWin.WndProc,
		}
	));

	private static readonly Lazy<string> popupWindow = new(() => RegisterClassUtils.Register(
		"PopupWindow",
		new RegisterClassParams
		{
			Styles =
				WindowClassStyles.CS_HREDRAW |
				WindowClassStyles.CS_VREDRAW |
				0
			,
			BackgroundBrush = Gdi32Helpers.GetStockObject(StockObject.BLACK_BRUSH),
			WinProc = SysWin.WndProc,
		}
	));
}