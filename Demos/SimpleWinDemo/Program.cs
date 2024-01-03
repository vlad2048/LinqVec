using GeomInt;
using RenderLib.Utils;
using WinAPI.Windows;
using SysWinLib;
using SysWinLib.Defaults;
using SysWinLib.Structs;
using WinAPI.User32;

namespace SimpleWinDemo;

static class Program
{
	private static readonly Brush backBrush = new SolidBrush(Color.DarkGray);

	private static void Main()
	{
		var win = new SysWin(opt =>
		{
			opt.NCStrat = NCStrats.None;
			opt.WinClass = WinClasses.MainWindow;
			opt.CreateWindowParams = new CreateWindowParams
			{
				Name = "SimpleWinDemo",
				X = 100,
				Y = 50,
				Width = 640,
				Height = 480,
				Styles = WindowStyles.WS_OVERLAPPEDWINDOW | WindowStyles.WS_VISIBLE
			};
		});

		win.WhenMsg.WhenPAINT().Subscribe(e =>
		{
			using var gfx = Graphics.FromHwnd(e.Hwnd);
			var r = new RInt(0, 0, win.ClientSz.V.X, win.ClientSz.V.Y);
			gfx.FillRectangle(backBrush, r.ToDrawRect());
		});

		win.Init();

		App.Run();
	}
}